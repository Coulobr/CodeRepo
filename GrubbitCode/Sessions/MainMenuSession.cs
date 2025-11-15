namespace Grubbit
{
	public class MainMenuSession : GrubbitSession
	{
		public override void LoadSession()
		{
			base.LoadSession();
			MainMenuUI.Open();
		}

		public override void UnloadSession()
		{
			base.UnloadSession();
			MainMenuUI.Close();
		}
	}
}