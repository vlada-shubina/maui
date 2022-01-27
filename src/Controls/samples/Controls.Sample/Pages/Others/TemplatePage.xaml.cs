using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class TemplatePage
	{
		public TemplatePage()
		{
			InitializeComponent();

			Update.Clicked += (sender, args) => {
				var value = double.Parse(ProgressValue.Text);
				Progress.Progress = value;
			};
		}
	}
}