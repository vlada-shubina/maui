﻿using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class Startup : IStartup
	{
		internal static bool UseBlazor = false;

		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.UseMauiApp<App>();
		}
	}
}
