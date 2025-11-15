using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Services.LevelPlay;

public class GameManagerUI : MonoBehaviour
{
	[Header("Refs")] public UIGridSystem grid;
	public UIBlockPool blockPool;
	public PieceSetSO pieceSet;
	public UIGhostRenderer ghost; // optional
	public UINextQueue nextQueueUI; // optional

	[Header("Cinemachine FX")] public CinemachineCamera cam; // assign your main VCam
	public CinemachineImpulseSource impulse; // add CinemachineImpulseSource on any object & assign here
	public float cameraZoomAmount = 0.6f; // how much to zoom (orthographicSize delta)
	public float cameraZoomDuration = 0.15f; // in seconds
	public float impulseStrength = 1.0f; // rumble strength

	[Header("Gameplay")] public float baseTick = 0.9f;
	public float minTick = 0.15f;
	public float softDropSeconds = 0.05f;
	public int mergeThreshold = 3; // (ValueChain) >= this many neighbors to merge

	[Header("Level Curve")] public int linesPerLevel = 10;
	public int level = 1;
	public int linesClearedTotal;
	private int _linesInLevel;

	[Header("Scoring")] public int score;
	public int singleLine = 100, doubleLine = 300, tripleLine = 500, tetrisLine = 800;
	public int mergePointFactor = 10; // result * count * factor * level

	[Header("Preview / Queue")] public int previewCount = 5;

	[Header("Spawn Weights (for variants)")]
	public int[] variantWeights = new int[] { 50, 30, 15, 4, 1 };

	[Header("Value → Data (optional visuals)")]
	public BlockDataSO[] valueToData; // index 0 => value 1

	// ---- internals ----
	private ActivePiece _active;
	private int _rotation;
	private float _dropTimer;
	private bool _isSoftDropping;
	private bool _isGameOver;
	private float _currentTick;
	private readonly List<UIBlockRuntime> _activeVisual = new();

	// 7-bag + next queue
	private readonly Queue<PieceDefinitionSO> _bag = new();

	[System.Serializable]
	public class PreviewPiece
	{
		public PieceDefinitionSO def;
		public List<BlockDataSO> cellData;
	}

	private readonly List<PreviewPiece> _next = new();

	// merge-event queue
	private readonly Queue<MergeEvent> _mergeQueue = new();

	// touch
	private bool _touchSoftDropping;

	public enum MergeMode
	{
		ValueChain,
		ColorSum
	} // 1→2→3 vs. color-based sum

	[Header("Merge Mode")] public MergeMode mergeMode = MergeMode.ValueChain;

	[Header("Color-Sum tuning")] [Tooltip("Minimum same-color blocks to trigger a Color-Sum merge.")]
	public int colorMergeThreshold = 3;

	[Tooltip("Score multiplier bump per block beyond the threshold (Color-Sum).")]
	public float perExtraCountMultiplier = 0.15f; // e.g., 3 blocks => x1.00, 4 => x1.15, 5 => x1.30...

	[Header("Color Set (7 colors)")] [Tooltip("List of the 7 color BlockDataSOs (each with colorMerge + spawnWeight).")]
	public BlockDataSO[] colorBlocks; // size = 7

	[Header("Adaptive Spawning")] private SpawnValueDirector valueDirector; // optional; if null we’ll emulate avg*0.9
	[Header("Merge Safety")]
	[Tooltip("If true, same-color merges also require the exact same BlockDataSO asset.")]
	public bool requireSameSOForColorMerge = false;

	[Header("Points FX")]
	public ScoreFXManager scoreFX;

	// --- Touch/Swipe tuning ---
	[Header("Touch Controls")]
	[Tooltip("Pixel distance to consider a gesture a swipe (screen space).")]
	public float swipeMinPixels = 20f;

	[Tooltip("Max pixel travel to still count as a tap (screen space).")]
	public float tapMaxPixels = 10f;

	[Tooltip("How many cell-widths of horizontal drag needed to step one column.")]
	public float swipeStepCells = 0.75f;

	[Tooltip("Middle band for soft-drop by tap/hold (normalized board width).")]
	public Vector2 middleBand = new Vector2(0.35f, 0.65f);

	[Tooltip("Soft-drop boost duration when middle is tapped (seconds). Hold also works).")]
	public float tapSoftDropBoost = 0.25f;

	// runtime touch state
	bool _pointerDown;
	bool _draggingHoriz;
	Vector2 _downScreen;
	Vector2 _downLocal;
	Vector2 _lastStepLocal;
	float _softDropTapTimer;


	void Start()
	{
		if (!grid) grid = FindFirstObjectByType<UIGridSystem>();
		grid.Init();
		grid.ReapplySizesForAllBlocks();

		if (!nextQueueUI) nextQueueUI = FindFirstObjectByType<UINextQueue>();
		if (!cam) cam = FindFirstObjectByType<CinemachineCamera>();
		if (!impulse) impulse = FindFirstObjectByType<CinemachineImpulseSource>();

		// Ensure we have a value director (optional but useful)
		if (!valueDirector) valueDirector = GetComponent<SpawnValueDirector>();
		if (!valueDirector) valueDirector = gameObject.AddComponent<SpawnValueDirector>();

		GameEvents.OnResetGame += ResetGame;

		LevelPlay.Init("MergeTrix");
		LevelPlay.ValidateIntegration();

		valueDirector.ResetAvg(1);
		ValidateColorKeys();

		FillNextQueue(previewCount);
		UpdateTickForLevel();

		StartCoroutine(GameLoop());
	}

	IEnumerator GameLoop()
	{
		ResetRun();
		GameEvents.OnGameStarted?.Invoke();

		while (!_isGameOver)
		{
			if (!TrySpawnPiece())
			{
				_isGameOver = true;
				break;
			}

			GameEvents.OnPieceSpawned?.Invoke();

			yield return HandlePieceControl();

			LandPiece();
			GameEvents.OnPieceLanded?.Invoke();

			// resolve merges via queued, juicy events
			yield return ResolveEventsJuicy();

			// keep the director updated with latest board average (after merges/lines)
			if (valueDirector) valueDirector.ObserveBoardAverage(GetBoardAverage());

			if (!AnySpawnPossible())
			{
				_isGameOver = true;
				GameEvents.OnGameOver?.Invoke();
				yield return new WaitForSeconds(2.0f);
				GameEvents.OnResetGame?.Invoke();
			}
		}
	}

	private void ResetGame()
	{
		StopAllCoroutines();
		StartCoroutine(GameLoop());
	}

	void ResetRun()
	{
		_isGameOver = false;
		score = 0;
		level = 1;
		_linesInLevel = 0;
		linesClearedTotal = 0;
		UpdateTickForLevel();
		GameEvents.OnScoreChanged?.Invoke(score);
		foreach (var v in _activeVisual)
			if (v)
				v.Despawn();
		_activeVisual.Clear();
		_mergeQueue.Clear();
		ghost?.Hide();
		if (valueDirector) valueDirector.ResetAvg(1);
	}

	void UpdateTickForLevel()
	{
		float t = Mathf.Clamp01((level - 1) / 20f);
		_currentTick = Mathf.Lerp(baseTick, minTick, t);
	}

	// ---------- Bag + Next ----------
	void RefillBag()
	{
		var list = new List<PieceDefinitionSO>(pieceSet.pieces);
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = Random.Range(0, i + 1);
			(list[i], list[j]) = (list[j], list[i]);
		}

		foreach (var p in list) _bag.Enqueue(p);
	}

	void ValidateColorKeys()
	{
		var seen = new HashSet<string>();
		foreach (var def in pieceSet.pieces)
		{
			if (def == null || def.variants == null) continue;
			foreach (var d in def.variants)
			{
				if (d == null) { Debug.LogWarning($"[ColorMerge] Null variant in {def.name}"); continue; }
				var k = (d.colorMerge ?? "").Trim().ToLowerInvariant();
				if (string.IsNullOrEmpty(k))
					Debug.LogWarning($"[ColorMerge] Empty colorMerge on BlockDataSO '{d.name}'. Set a key like 'red/green/...'.");
				else if (!seen.Contains(k)) seen.Add(k);
			}
		}
	}


	PieceDefinitionSO DrawFromBag()
	{
		if (_bag.Count == 0) RefillBag();
		return _bag.Dequeue();
	}

	void FillNextQueue(int desired)
	{
		while (_next.Count < desired)
		{
			var def = DrawFromBag();
			// for preview we keep the simple per-cell pick; actual spawn will re-pick safely
			var data = ChoosePerCellData(def);
			_next.Add(new PreviewPiece { def = def, cellData = data });
		}

		nextQueueUI?.SetQueuePreview(_next);
	}

	// ---------- Spawn ----------
	bool TrySpawnPiece()
	{
		if (_next.Count == 0) FillNextQueue(previewCount);

		var pp = _next[0];
		_next.RemoveAt(0);

		var def = pp.def;
		var anchor = ComputeSafeSpawnAnchor(def, 0);

		// Use the new def-based drop anchor (no _active needed yet)
		var dropAnchor = FindDropAnchor(def, anchor, 0);

		// Recompute per-cell COLORS safely for the intended landing position
		var safeCellData = ChoosePerCellDataSafe(def, dropAnchor, 0);

		// Collision test at the initial spawn location
		var testCells = CellsWorld(def, anchor, 0).ToArray();
		foreach (var c in testCells)
			if (!grid.IsEmpty(c))
				return false;

		// Now create the active piece
		_active = new ActivePiece { def = def, anchor = anchor, cellData = safeCellData };
		_rotation = 0;
		_dropTimer = 0f;
		_isSoftDropping = false;

		// Backfill & draw queue
		FillNextQueue(previewCount);
		nextQueueUI?.SetQueuePreview(_next);

		SpawnActiveVisuals(); // sets values via director (~avg -10%)
		UpdateActiveVisualPositions(true);
		UpdateGhost();
		return true;
	}

	Vector2Int ComputeSafeSpawnAnchor(PieceDefinitionSO def, int rotation = 0)
	{
		int maxY = int.MinValue;
		foreach (var c in def.localCells)
		{
			var r = RotateAroundPivot(c, def.pivot, rotation);
			if (r.y > maxY) maxY = r.y;
		}

		int spawnY = grid.height - 1 - maxY;
		int spawnX = Mathf.Clamp(grid.width / 2 - 1, 0, Mathf.Max(0, grid.width - 2));
		return new Vector2Int(spawnX, spawnY);
	}

	static Vector2Int RotateAroundPivot(in Vector2Int c, in Vector2 pivot, int rotation)
	{
		float dx = c.x - pivot.x;
		float dy = c.y - pivot.y;
		float rx, ry;
		switch (rotation & 3)
		{
			case 1:
				rx = -dy;
				ry = dx;
				break;
			case 2:
				rx = -dx;
				ry = -dy;
				break;
			case 3:
				rx = dy;
				ry = -dx;
				break;
			default:
				rx = dx;
				ry = dy;
				break;
		}

		int gx = Mathf.RoundToInt(rx + pivot.x);
		int gy = Mathf.RoundToInt(ry + pivot.y);
		return new Vector2Int(gx, gy);
	}

	bool AnySpawnPossible()
	{
		foreach (var def in pieceSet.pieces)
		{
			var anchor = ComputeSafeSpawnAnchor(def, 0);
			var cells = new ActivePiece { def = def }.CellsWorld(anchor, 0).ToArray();
			bool blocked = false;
			foreach (var c in cells)
				if (!grid.IsEmpty(c))
				{
					blocked = true;
					break;
				}

			if (!blocked) return true;
		}

		return false;
	}

	// ---------- Active visuals ----------
	void SpawnActiveVisuals()
	{
		foreach (var v in _activeVisual)
			if (v)
				v.Despawn();
		_activeVisual.Clear();

		int spawnV = valueDirector ? valueDirector.NextSpawnValue()
			: Mathf.Max(1, Mathf.RoundToInt(GetBoardAverage() * 0.9f));


		var cells = _active.CellsWorld(_active.anchor, _rotation);
		for (int i = 0; i < cells.Count; i++)
		{
			var b = blockPool.Get();
			b.transform.SetParent(grid.boardRect, false);

			var data = _active.cellData[Mathf.Min(i, _active.cellData.Count - 1)];
			b.Init(data);
			b.SetValue(spawnV); // <-- adaptive spawn value here
			b.JustPlaced = true;

			grid.ApplyBlockMetrics(b);
			_activeVisual.Add(b);
		}
	}

	void UpdateActiveVisualPositions(bool immediate = false)
	{
		var cells = _active.CellsWorld(_active.anchor, _rotation);
		for (int i = 0; i < _activeVisual.Count && i < cells.Count; i++)
		{
			var anchored = grid.GridToAnchored(cells[i]);
			if (immediate) _activeVisual[i].Rect.anchoredPosition = anchored;
			else _activeVisual[i].AnimateFallTo(anchored);
		}

		UpdateGhost();
	}

	// ---------- Controls ----------
	IEnumerator HandlePieceControl()
	{
		while (true)
		{
			bool changed = false;
			if (Input.GetKeyDown(KeyCode.LeftArrow)) changed |= TryMove(new Vector2Int(-1, 0));
			if (Input.GetKeyDown(KeyCode.RightArrow)) changed |= TryMove(new Vector2Int(1, 0));
			if (Input.GetKeyDown(KeyCode.Z)) changed |= TryRotate(-1);
			if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.UpArrow)) changed |= TryRotate(1);

			HandleTouchInput();

			if (changed) UpdateActiveVisualPositions();

			// decay any tap-based soft drop boost
			if (_softDropTapTimer > 0f) _softDropTapTimer -= Time.deltaTime;
			_isSoftDropping = _touchSoftDropping || Input.GetKey(KeyCode.DownArrow) || (_softDropTapTimer > 0f);
			float targetTick = _isSoftDropping ? softDropSeconds : _currentTick;
			_dropTimer += Time.deltaTime;
			if (_dropTimer >= targetTick)
			{
				_dropTimer = 0f;
				if (!TryMove(new Vector2Int(0, -1))) yield break; // lock piece
				UpdateActiveVisualPositions();
			}

			yield return null;
		}
	}

	void HandleTouchInput()
	{
		if (!grid || !grid.boardRect) return;

		bool pressedThisFrame = false;
		bool releasedThisFrame = false;
		bool holding = false;
		Vector2 screenPos = default;

#if UNITY_EDITOR || UNITY_STANDALONE
		if (Input.GetMouseButtonDown(0)) { pressedThisFrame = true; screenPos = Input.mousePosition; }
		if (Input.GetMouseButton(0)) { holding = true; screenPos = Input.mousePosition; }
		if (Input.GetMouseButtonUp(0)) { releasedThisFrame = true; screenPos = Input.mousePosition; }
#else
    if (Input.touchCount > 0)
    {
        var t = Input.GetTouch(0);
        screenPos = t.position;
        pressedThisFrame  = (t.phase == TouchPhase.Began);
        releasedThisFrame = (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled);
        holding           = (t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved);
    }
#endif

		// Convert to board-local space (anchored)
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(grid.boardRect, screenPos, null, out var localPoint))
		{
			_touchSoftDropping = false;
			return;
		}

		// Normalized coords inside board
		var half = grid.boardRect.sizeDelta * 0.5f;
		float nx = Mathf.InverseLerp(-half.x, half.x, localPoint.x);
		float ny = Mathf.InverseLerp(-half.y, half.y, localPoint.y);
		bool inside = nx >= 0f && nx <= 1f && ny >= 0f && ny <= 1f;
		if (!inside)
		{
			_touchSoftDropping = false;
			// Reset state if finger leaves board and releases
			if (releasedThisFrame) { _pointerDown = false; _draggingHoriz = false; }
			return;
		}

		// Cell width in local pixels for step thresholds
		float cellW = (grid.boardRect.sizeDelta.x / Mathf.Max(1, grid.width));
		float stepThreshold = Mathf.Max(4f, cellW * swipeStepCells); // guard for tiny boards
		float minSwipePixels = Mathf.Max(8f, swipeMinPixels);
		float maxTapPixels = Mathf.Max(4f, tapMaxPixels);

		bool inMiddle = nx >= middleBand.x && nx <= middleBand.y;

		// Pointer down: initialize gesture state
		if (pressedThisFrame)
		{
			_pointerDown = true;
			_draggingHoriz = false;
			_downScreen = screenPos;
			_downLocal = localPoint;
			_lastStepLocal = localPoint;

			// If tapped inside middle band, start a short soft-drop boost (also see 'holding' below)
			if (inMiddle) _softDropTapTimer = tapSoftDropBoost;
		}

		if (_pointerDown && holding)
		{
			var screenDelta = (Vector2)screenPos - _downScreen;
			// Decide if this gesture becomes a horizontal drag
			if (!_draggingHoriz)
			{
				if (Mathf.Abs(screenDelta.x) > minSwipePixels && Mathf.Abs(screenDelta.x) > Mathf.Abs(screenDelta.y))
				{
					_draggingHoriz = true;
					_lastStepLocal = localPoint; // start stepping from here
				}
			}

			// While dragging horizontally, step left/right as we cross thresholds
			if (_draggingHoriz)
			{
				float dx = localPoint.x - _lastStepLocal.x;
				// Move right
				while (dx > stepThreshold)
				{
					if (TryMove(new Vector2Int(1, 0))) UpdateActiveVisualPositions();
					_lastStepLocal.x += stepThreshold;
					dx -= stepThreshold;
				}
				// Move left
				while (dx < -stepThreshold)
				{
					if (TryMove(new Vector2Int(-1, 0))) UpdateActiveVisualPositions();
					_lastStepLocal.x -= stepThreshold;
					dx += stepThreshold;
				}
			}
		}

		// Soft drop from holding in middle band (continuous)
		_touchSoftDropping = holding && inMiddle;

		// On release: if it was a tap (not a horizontal drag), handle rotate/soft drop tap
		if (releasedThisFrame && _pointerDown)
		{
			var totalScreenDelta = (Vector2)screenPos - _downScreen;
			bool wasTap = totalScreenDelta.magnitude <= maxTapPixels && !_draggingHoriz;

			if (wasTap)
			{
				if (inMiddle)
				{
					// already nudged _softDropTapTimer on press; nothing else needed
				}
				else
				{
					// rotate on tap outside middle band
					if (TryRotate(1)) UpdateActiveVisualPositions();
				}
			}

			_pointerDown = false;
			_draggingHoriz = false;
		}
	}


	bool TryMove(Vector2Int delta)
	{
		var next = _active.anchor + delta;
		var cells = _active.CellsWorld(next, _rotation).ToArray();
		if (!Fits(cells)) return false;
		_active.anchor = next;
		return true;
	}

	bool TryRotate(int dir)
	{
		int tryRot = (_rotation + dir) & 3;
		Vector2Int[] kicks =
		{
			new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 0),
			new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 1),
			new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)
		};

		foreach (var k in kicks)
		{
			var nextAnchor = _active.anchor + k;
			var cells = _active.CellsWorld(nextAnchor, tryRot).ToArray();
			if (Fits(cells))
			{
				_rotation = tryRot;
				_active.anchor = nextAnchor;
				return true;
			}
		}

		return false;
	}

	bool Fits(System.ReadOnlySpan<Vector2Int> cells)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			var c = cells[i];
			if (!grid.InBounds(c)) return false;
			if (grid.Get(c) != null) return false;
		}

		return true;
	}

	void LandPiece()
	{
		var cells = _active.CellsWorld(_active.anchor, _rotation);
		for (int i = 0; i < _activeVisual.Count && i < cells.Count; i++)
		{
			var b = _activeVisual[i];
			grid.Place(b, cells[i]);
			b.JustPlaced = false;
		}

		_activeVisual.Clear();
		ghost?.Hide();
	}

	// ======== MERGE SYSTEM (juicy) ========

	struct Group
	{
		public int value;
		public List<Vector2Int> positions;
	}

	class MergeEvent
	{
		public int value; // informational
		public List<Vector2Int> positions; // all positions in the group
		public Vector2Int center; // chosen survivor cell
		public int resultValue; // ValueChain: v+1, ColorSum: sum
	}
	enum BoardEventType { Merge, LineClear }

	class LineClearEvent
	{
		public BoardEventType type = BoardEventType.LineClear;
		public List<int> rows;                 // cleared row indices
		public List<Vector2Int> allPositions;  // all cells that will be cleared (for animation)
		public Vector2Int bannerCenter;        // where to show EXCELLENT banner
		public int lines;                      // row count
		public int points;                     // computed points to award
	}
	List<LineClearEvent> ComputeLineClearEventsAndScore()
	{
		var rows = grid.GetFullRows();
		var result = new List<LineClearEvent>();
		if (rows == null || rows.Count == 0) return result;

		rows.Sort(); // from bottom up
		// Sum of block values in ALL cleared rows (for scoring)
		int sumValues = 0;
		var allPos = new List<Vector2Int>();

		foreach (var y in rows)
		{
			for (int x = 0; x < grid.width; x++)
			{
				var p = new Vector2Int(x, y);
				var b = grid.Get(p);
				if (b != null) sumValues += Mathf.Max(0, b.Value);
				allPos.Add(p);
			}
		}

		// multipliers per spec
		int lines = rows.Count;
		int mult = (lines >= 4) ? 50 : (lines == 3 ? 20 : (lines == 2 ? 10 : 5));
		int points = sumValues * mult;

		// center banner near the middle cleared row
		int midRow = rows[rows.Count / 2];
		var bannerCenter = new Vector2Int(grid.width / 2, midRow);

		result.Add(new LineClearEvent
		{
			rows = rows,
			allPositions = allPos,
			bannerCenter = bannerCenter,
			lines = lines,
			points = points
		});

		return result;
	}
	IEnumerator PlayLineClearEventJuicy(LineClearEvent e)
	{
		// 1) Animate blocks on those rows: pulse + slight upward drift + fade
		float pulse = 0.15f;
		foreach (var p in e.allPositions)
		{
			var b = grid.Get(p);
			if (b == null) continue;
			b.Rect.DOPunchScale(Vector3.one * 0.25f, pulse, 10, 0.9f);
			var rt = b.Rect;
			var start = rt.anchoredPosition;
			rt.DOAnchorPos(start + new Vector2(0f, 30f), pulse + 0.1f).SetEase(Ease.OutQuad);
			b.AnimateMerge(); // reuse your merge SFX/juice if present
		}
		yield return new WaitForSeconds(pulse + 0.05f);

		// 2) Remove the rows from the grid, but let the visuals fade nicely
		foreach (var y in e.rows)
		{
			for (int x = 0; x < grid.width; x++)
			{
				var p = new Vector2Int(x, y);
				var b = grid.Get(p);
				if (b != null)
				{
					grid.Remove(p);
					b.AnimateFadeAndDespawn(0.18f);
				}
			}
		}
		yield return new WaitForSeconds(0.18f);

		// 3) Award points and banner
		var bannerAnchored = grid.GridToAnchored(e.bannerCenter);
		if (scoreFX != null)
		{
			scoreFX.ShowBannerUpperCenter("EXCELLENT");
			scoreFX.AwardPoints(e.points, bannerAnchored);
		}
		score += e.points;
		linesClearedTotal += e.lines;
		_linesInLevel += e.lines;

		GameEvents.OnLinesCleared?.Invoke(e.lines);
		GameEvents.OnScoreChanged?.Invoke(score);

		// 4) Level up check (keep your pacing)
		if (_linesInLevel >= linesPerLevel)
		{
			_linesInLevel -= linesPerLevel;
			level++;
			UpdateTickForLevel();
		}

		// 5) Collapse rows (your grid utility already re-packs after clears)
		grid.CollapseRows(e.rows);
		yield return new WaitForSeconds(0.05f);

		// 6) Gravity pass to settle any above blocks (if needed)
		yield return GravityPass();
	}

	IEnumerator ResolveEventsJuicy()
	{
		while (true)
		{
			bool didSomething = false;

			// Track this pass
			int mergeComboThisPass = 0;       // how many merges resolved this pass
			bool lineClearThisPass = false;   // did we clear any lines this pass?

			// ===== MERGES =====
			var merges = ComputeMergeEvents(ignoreJustPlaced: true);
			if (merges.Count > 0)
			{
				foreach (var m in merges) _mergeQueue.Enqueue(m);
				while (_mergeQueue.Count > 0)
				{
					var e = _mergeQueue.Dequeue();
					yield return PlayMergeEventJuicy(e);
					mergeComboThisPass++;
					yield return new WaitForSeconds(0.03f);
					yield return GravityPass();
				}
				didSomething = true;
			}

			// ===== LINE CLEARS (override banners) =====
			var clears = ComputeLineClearEventsAndScore();
			if (clears.Count > 0)
			{
				lineClearThisPass = true; // <-- mark override
				foreach (var lc in clears)
				{
					yield return PlayLineClearEventJuicy(lc); // shows EXCELLENT inside
				}
				didSomething = true;
			}

			// ===== SHOW BANNER ONCE (only if NO line clear happened) =====
			if (!lineClearThisPass && mergeComboThisPass >= 2 && scoreFX != null)
			{
				string label = (mergeComboThisPass == 2) ? "NICE"
					: (mergeComboThisPass == 3) ? "GREAT"
					: "AWESOME";
				scoreFX.ShowBannerUpperCenter(label);
			}

			if (!didSomething) break;
			yield return new WaitForSeconds(0.03f);
		}
	}


	void ShowComboBannerIfAny(int combo, Vector2Int gridCenterPos)
	{
		if (combo <= 1 || scoreFX == null) return;

		string label = (combo == 2) ? "NICE" : (combo == 3) ? "GREAT" : "AWESOME";
		var anchored = grid.GridToAnchored(gridCenterPos);
		scoreFX.ShowBannerUpperCenter(label);
	}

	List<MergeEvent> ComputeMergeEvents(bool ignoreJustPlaced)
	{
		var visited = new HashSet<Vector2Int>();
		var taken = new HashSet<Vector2Int>(); // prevent overlaps in this pass
		var result = new List<MergeEvent>();

		foreach (var p in grid.AllPositions())
		{
			if (visited.Contains(p)) continue;
			var b = grid.Get(p);
			if (b == null) continue;
			if (ignoreJustPlaced && b.JustPlaced) continue;
			visited.Add(p);

			// BFS collect group
			var q = new Queue<Vector2Int>();
			var members = new List<Vector2Int> { p };
			q.Enqueue(p);

			if (mergeMode == MergeMode.ValueChain)
			{
				int val = b.Value;
				while (q.Count > 0)
				{
					var cur = q.Dequeue();
					foreach (var n in Neighbors4(cur))
					{
						if (!grid.InBounds(n) || visited.Contains(n)) continue;
						var nb = grid.Get(n);
						if (nb != null && !(ignoreJustPlaced && nb.JustPlaced) && nb.Value == val)
						{
							visited.Add(n);
							q.Enqueue(n);
							members.Add(n);
						}
					}
				}

				if (members.Count >= mergeThreshold)
				{
					if (!OverlapsTaken(members, taken))
					{
						members.Sort((a, b2) => a.y == b2.y ? a.x.CompareTo(b2.x) : a.y.CompareTo(b2.y));
						var center = members[members.Count / 2];
						result.Add(new MergeEvent
						{
							value = b.Value,
							positions = members,
							center = center,
							resultValue = NextValue(b.Value) // v+1
						});
						MarkTaken(members, taken);
					}
				}
			}
			else // Color-Sum (by color identity)
			{
				string key = GetColorKey(b);
				while (q.Count > 0)
				{
					var cur = q.Dequeue();
					foreach (var n in Neighbors4(cur))
					{
						if (!grid.InBounds(n) || visited.Contains(n)) continue;
						var nb = grid.Get(n);
						if (nb != null && !(ignoreJustPlaced && nb.JustPlaced))
						{
							bool sameKey = GetColorKey(nb) == key;
							bool sameSO = (!requireSameSOForColorMerge) || (nb.Data == b.Data);
							if (sameKey && sameSO)
							{
								visited.Add(n);
								q.Enqueue(n);
								members.Add(n);
							}
						}
					}
				}

				if (members.Count >= colorMergeThreshold)
				{
					if (!OverlapsTaken(members, taken))
					{
						members.Sort((a, b2) => a.y == b2.y ? a.x.CompareTo(b2.x) : a.y.CompareTo(b2.y));
						var center = members[members.Count / 2];

						// Sum all values in the same-color group
						int sum = 0;
						foreach (var pos in members)
						{
							var nb = grid.Get(pos);
							if (nb != null) sum += nb.Value;
						}

						result.Add(new MergeEvent
						{
							value = b.Value, // informational
							positions = members,
							center = center,
							resultValue = Mathf.Max(1, sum) // result = SUM(values)
						});
						MarkTaken(members, taken);
					}
				}
			}
		}

		return result;

		// local helpers
		static bool OverlapsTaken(List<Vector2Int> cells, HashSet<Vector2Int> taken)
		{
			foreach (var c in cells)
				if (taken.Contains(c))
					return true;
			return false;
		}

		static void MarkTaken(List<Vector2Int> cells, HashSet<Vector2Int> taken)
		{
			foreach (var c in cells) taken.Add(c);
		}
	}

	static IEnumerable<Vector2Int> Neighbors4(Vector2Int p)
	{
		yield return new Vector2Int(p.x + 1, p.y);
		yield return new Vector2Int(p.x - 1, p.y);
		yield return new Vector2Int(p.x, p.y + 1);
		yield return new Vector2Int(p.x, p.y - 1);
	}

	int NextValue(int v) => v + 1; // 1->2->3 progression (ValueChain)

	IEnumerator PlayMergeEventJuicy(MergeEvent e)
	{
		// Recollect blocks that STILL exist at the recorded positions
		var entries = new List<(Vector2Int pos, UIBlockRuntime blk)>(e.positions.Count);
		foreach (var pos in e.positions)
		{
			var b = grid.Get(pos);
			if (b != null) entries.Add((pos, b));
		}

		// If we lost too many members, abort this event
		int threshold = (mergeMode == MergeMode.ValueChain) ? mergeThreshold : colorMergeThreshold;
		if (entries.Count < threshold) yield break;

		// Pick a center that still exists (prefer original, else middle of remaining)
		Vector2Int centerPos = e.center;
		int centerIdx = entries.FindIndex(t => t.pos == centerPos);
		if (centerIdx < 0) centerIdx = Mathf.Clamp(entries.Count / 2, 0, entries.Count - 1);
		centerPos = entries[centerIdx].pos;

		var centerBlock = entries[centerIdx].blk;
		if (centerBlock == null) yield break;

		// Camera FX
		PlayMergeCameraFX(e);

		// Animate: everyone moves/pulses toward the center (visual only, grid unchanged yet)
		Vector2 centerAnchored = grid.GridToAnchored(centerPos);
		float moveDur = 0.15f;
		for (int i = 0; i < entries.Count; i++)
		{
			var (pos, blk) = entries[i];
			if (blk == null) continue;
			blk.AnimateMoveTo(centerAnchored, moveDur);
			blk.AnimateMerge();
		}

		yield return new WaitForSeconds(moveDur + 0.02f);

		// Remove non-center participants from the grid (if still registered there), then fade out
		for (int i = 0; i < entries.Count; i++)
		{
			if (i == centerIdx) continue;
			var (pos, blk) = entries[i];
			if (blk == null) continue;

			var cur = grid.Get(pos);
			if (cur == blk) grid.Remove(pos); // only remove if the same block is still at that cell
			blk.AnimateFadeAndDespawn(0.10f); // fades, then returns to pool
		}

		yield return new WaitForSeconds(0.10f);

		// If the center is still the same block at that cell, upgrade/value-change + score
		var stillCenter = grid.Get(centerPos);
		if (stillCenter == centerBlock)
		{
			// Upgrade visuals/number (resultValue was computed in ComputeMergeEvents)
			centerBlock.SetValue(e.resultValue, GetDataForValue(e.resultValue));

			// ---------- SCORING ----------
			int add;
			if (mergeMode == MergeMode.ValueChain)
			{
				// Classic: result * count * factor * level
				add = e.resultValue * entries.Count * mergePointFactor * level;
			}
			else // ColorSum
			{
				// Base = sum of consumed values
				int baseSum = 0;
				for (int i = 0; i < entries.Count; i++)
				{
					var blk = entries[i].blk;
					if (blk != null) baseSum += blk.Value;
				}

				// Multiplier grows per extra block over threshold
				int extra = Mathf.Max(0, entries.Count - colorMergeThreshold);
				float countMult = 1f + perExtraCountMultiplier * extra;

				// Color frequency factor (avg of participants); set per BlockDataSO
				float freq = 0f;
				int freqN = 0;
				for (int i = 0; i < entries.Count; i++)
				{
					var blk = entries[i].blk;
					if (blk != null)
					{
						freq += GetFrequencyFactorFor(blk);
						freqN++;
					}
				}

				float freqFactor = (freqN > 0) ? (freq / freqN) : 1f;

				add = Mathf.RoundToInt(baseSum * countMult * freqFactor * level);
			}

			// Show popup at the merge center and let the counter tally
			if (scoreFX != null)
			{
				// e.center is grid coords; convert once to anchored (you already had centerAnchored above)
				scoreFX.AwardPoints(add, centerAnchored);
				// Keep the logical score in sync immediately (so gameplay isn’t delayed by UI)
				score += add;
				GameEvents.OnScoreChanged?.Invoke(score);
			}
			else
			{
				score += add;
				GameEvents.OnScoreChanged?.Invoke(score);
			}

			GameEvents.OnBlocksMerged?.Invoke(e.resultValue, entries.Count);
		}
	}

	void PlayMergeCameraFX(MergeEvent e)
	{
		if (impulse) impulse.GenerateImpulse(impulseStrength); // impulse rumble

		if (cam)
		{
			StartCoroutine(ZoomRoutine()); // quick zoom (orthographic)
		}
	}

	IEnumerator ZoomRoutine()
	{
		if (cam == null) yield break;
		float baseSize = cam.Lens.OrthographicSize;
		float target = Mathf.Max(0.01f, baseSize - cameraZoomAmount);
		float t = 0f;

		// zoom in
		while (t < cameraZoomDuration)
		{
			t += Time.deltaTime;
			float a = t / cameraZoomDuration;
			var lens = cam.Lens;
			lens.OrthographicSize = Mathf.Lerp(baseSize, target, a);
			cam.Lens = lens;
			yield return null;
		}

		// zoom back
		t = 0f;
		while (t < cameraZoomDuration)
		{
			t += Time.deltaTime;
			float a = t / cameraZoomDuration;
			var lens = cam.Lens;
			lens.OrthographicSize = Mathf.Lerp(target, baseSize, a);
			cam.Lens = lens;
			yield return null;
		}
	}

	BlockDataSO GetDataForValue(int v)
	{
		if (valueToData != null && v > 0 && v <= valueToData.Length)
			return valueToData[v - 1];
		return null;
	}

	IEnumerator GravityPass()
	{
		for (int x = 0; x < grid.width; x++)
		{
			for (int y = 0; y < grid.height; y++)
			{
				var p = new Vector2Int(x, y);
				if (grid.Get(p) != null) continue;
				for (int yy = y + 1; yy < grid.height; yy++)
				{
					var from = new Vector2Int(x, yy);
					var b = grid.Get(from);
					if (b != null)
					{
						grid.Remove(from);
						grid.Place(b, p);
						b.AnimateFallTo(grid.GridToAnchored(p));
						break;
					}
				}
			}
		}

		yield return null;
	}

	// ----- simple preview per-cell pick (used only for queue visuals) -----
	List<BlockDataSO> ChoosePerCellData(PieceDefinitionSO def)
	{
		var result = new List<BlockDataSO>(def.localCells.Length);
		if (def.variants != null && def.variants.Length > 0)
		{
			for (int i = 0; i < def.localCells.Length; i++)
				result.Add(WeightedPick(def.variants));
		}
		else
		{
			for (int i = 0; i < def.localCells.Length; i++)
				result.Add(def.defaultBlockData);
		}

		return result;
	}

	BlockDataSO WeightedPick(BlockDataSO[] options)
	{
		if (options == null || options.Length == 0) return null;
		if (variantWeights == null || variantWeights.Length == 0)
			return options[Random.Range(0, options.Length)];

		int total = 0;
		int count = Mathf.Min(options.Length, variantWeights.Length);
		for (int i = 0; i < count; i++) total += Mathf.Max(0, variantWeights[i]);
		if (total <= 0) return options[0];

		int pick = Random.Range(0, total), acc = 0;
		for (int i = 0; i < count; i++)
		{
			acc += Mathf.Max(0, variantWeights[i]);
			if (pick < acc) return options[i];
		}

		return options[count - 1];
	}

	void UpdateGhost()
	{
		if (ghost == null || _active == null) return;
		var targetAnchor = FindDropAnchor(_active.anchor, _rotation);
		var cells = _active.CellsWorld(targetAnchor, _rotation);
		ghost.ShowAt(cells);
	}

	Vector2Int FindDropAnchor(Vector2Int startAnchor, int rotation)
	{
		var a = startAnchor;
		while (true)
		{
			var next = a + new Vector2Int(0, -1);
			var cells = _active.CellsWorld(next, rotation).ToArray();
			if (!Fits(cells)) return a;
			a = next;
		}
	}

	Vector2Int FindDropAnchor(PieceDefinitionSO def, Vector2Int startAnchor, int rotation)
	{
		var a = startAnchor;
		while (true)
		{
			var next = a + new Vector2Int(0, -1);
			var cells = CellsWorld(def, next, rotation).ToArray();
			if (!Fits(cells)) return a;
			a = next;
		}
	}
	// ---------- Helpers for adaptive value & color-safe spawns ----------

	float GetBoardAverage()
	{
		float sum = 0f;
		int n = 0;
		foreach (var p in grid.AllPositions())
		{
			var b = grid.Get(p);
			if (b != null)
			{
				sum += b.Value;
				n++;
			}
		}

		return n > 0 ? (sum / n) : 1f;
	}

	string GetColorKey(UIBlockRuntime b)
	{
		if (b == null || b.Data == null) return "";
		// normalize & forbid empty
		var k = (b.Data.colorMerge ?? "").Trim().ToLowerInvariant();
		return string.IsNullOrEmpty(k) ? $"__unset__:{b.Data.name}" : k;
	}


	// Pick per-cell BlockDataSOs for a piece such that placing them
	// at (anchor, rotation) won't create any immediate same-color group >= colorMergeThreshold
	// (considering board neighbors + within-piece already assigned neighbors).
	// Choose per-cell data from THIS PIECE'S variants, avoiding instant ≥ threshold merges at the landing position
	List<BlockDataSO> ChoosePerCellDataSafe(PieceDefinitionSO def, Vector2Int anchor, int rotation)
	{
		var worldCells = CellsWorld(def, anchor, rotation);
		var result = new List<BlockDataSO>(worldCells.Count);

		for (int i = 0; i < worldCells.Count; i++)
		{
			var targetPos = worldCells[i];
			const int MAX_TRIES = 16;
			BlockDataSO chosen = null;

			for (int attempt = 0; attempt < MAX_TRIES; attempt++)
			{
				var candidate = PickColorDataWeighted(def.variants, variantWeights);
				string key = candidate ? candidate.colorMerge : "none";

				if (!WouldCreateAutoMerge(targetPos, key, worldCells, result, i))
				{
					chosen = candidate;
					break;
				}
			}

			// Robust fallback chain: variants → default → null (will show white)
			if (chosen == null)
			{
				chosen = PickColorDataWeighted(def.variants, variantWeights);
				if (chosen == null) chosen = def.defaultBlockData;
				if (chosen == null)
					Debug.LogWarning("[GameManagerUI] No valid BlockData for a cell (variants & default null). It will look white.");
			}

			result.Add(chosen);
		}
		return result;
	}


	List<Vector2Int> CellsWorld(PieceDefinitionSO def, Vector2Int anchor, int rotation)
	{
		var list = new List<Vector2Int>(def.localCells.Length);
		for (int i = 0; i < def.localCells.Length; i++)
		{
			var r = RotateAroundPivot(def.localCells[i], def.pivot, rotation);
			list.Add(new Vector2Int(anchor.x + r.x, anchor.y + r.y));
		}

		return list;
	}

	// Check if assigning `colorKey` at `pos` would create a connected same-color group >= threshold
	// considering already-placed board blocks AND already-assigned cells in this piece.
	bool WouldCreateAutoMerge(Vector2Int pos, string colorKey, List<Vector2Int> worldCells, List<BlockDataSO> assigned,
		int uptoIndex)
	{
		var q = new Queue<Vector2Int>();
		var seen = new HashSet<Vector2Int>();
		q.Enqueue(pos);
		seen.Add(pos);
		int count = 0;

		while (q.Count > 0)
		{
			var cur = q.Dequeue();
			count++;

			if (count >= colorMergeThreshold) return true; // would auto-merge

			foreach (var n in Neighbors4(cur))
			{
				if (seen.Contains(n)) continue;

				// If neighbor is part of the CURRENT piece
				int idx = worldCells.IndexOf(n);
				if (idx >= 0)
				{
					if (idx < uptoIndex)
					{
						var dataAssigned = assigned[idx];
						string ak = dataAssigned ? dataAssigned.colorMerge : "none";
						if (ak == colorKey)
						{
							seen.Add(n);
							q.Enqueue(n);
						}
					}

					// idx == uptoIndex is "self" (already in seen)
					// idx > uptoIndex => not yet assigned; conservatively treat as different
					continue;
				}

				// Else, neighbor is on the BOARD
				var nb = grid.InBounds(n) ? grid.Get(n) : null;
				if (nb != null)
				{
					var nbKey = nb.Data ? nb.Data.colorMerge : "none";
					if (nbKey == colorKey)
					{
						seen.Add(n);
						q.Enqueue(n);
					}
				}
			}
		}

		return false; // safe
	}

	float GetFrequencyFactorFor(UIBlockRuntime b) => (b?.Data != null) ? b.Data.frequencyScoreFactor : 1f;

	// Weighted color pick from the 7 color identities (hardened)
	BlockDataSO PickColorDataWeighted()
	{
		if (colorBlocks == null || colorBlocks.Length == 0)
		{
			Debug.LogWarning(
				"[GameManagerUI] colorBlocks is empty — cannot pick colors. Assign 7 BlockDataSO in the Inspector.");
			return null;
		}

		// filter nulls and sum positive weights
		int total = 0;
		BlockDataSO lastNonNull = null;
		for (int i = 0; i < colorBlocks.Length; i++)
		{
			var d = colorBlocks[i];
			if (d == null) continue;
			lastNonNull = d;
			int w = Mathf.Max(0, d.spawnWeight);
			total += w;
		}

		if (lastNonNull == null)
		{
			Debug.LogWarning("[GameManagerUI] All entries in colorBlocks are null.");
			return null;
		}

		if (total <= 0)
		{
			Debug.LogWarning(
				"[GameManagerUI] All colorBlocks have non-positive spawnWeight. Using last non-null as fallback.");
			return lastNonNull;
		}

		int pick = Random.Range(0, total);
		int acc = 0;
		for (int i = 0; i < colorBlocks.Length; i++)
		{
			var d = colorBlocks[i];
			if (d == null) continue;
			int w = Mathf.Max(0, d.spawnWeight);
			acc += w;
			if (pick < acc) return d;
		}

		return lastNonNull;
	}
	// Generic weighted pick from an options array + optional weights
	BlockDataSO PickColorDataWeighted(BlockDataSO[] options, int[] weights = null)
	{
		if (options == null || options.Length == 0) return null;

		// If no weights passed, or lengths mismatch, pick uniformly
		if (weights == null || weights.Length < options.Length)
			return options[Random.Range(0, options.Length)];

		// Sum positive weights, remember last non-null
		int total = 0;
		BlockDataSO lastNonNull = null;
		int count = Mathf.Min(options.Length, weights.Length);
		for (int i = 0; i < count; i++)
		{
			var d = options[i];
			if (d == null) continue;
			lastNonNull = d;
			total += Mathf.Max(0, weights[i]);
		}
		if (lastNonNull == null) return null;
		if (total <= 0) return lastNonNull;

		int pick = Random.Range(0, total);
		int acc = 0;
		for (int i = 0; i < count; i++)
		{
			var d = options[i];
			if (d == null) continue;
			acc += Mathf.Max(0, weights[i]);
			if (pick < acc) return d;
		}
		return lastNonNull;
	}
}

