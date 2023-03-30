using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Interfaces;
using Jint;
using Jint.CommonJS;

namespace Infrastructure.Services
{
	public class TextSizeService : ITextSizeService
	{
		public (float, float) CalculateTextSize(string text, string font, int fontSize)
		{
			var engine = new Engine(options =>
			{
				options.EnableModules(Path.GetFullPath(@"..\Infrastructure\JavaScript"));
			});

			var exports = engine.CommonJS().RunMain("./opentype.module.js");

			var js = File.ReadAllText(@"..\Infrastructure\JavaScript\opentype.js");
			engine.Execute(js);
			engine.Execute("require('fs');");

			engine.SetValue("context", new
			{
				test = "yay"
			});
			//engine.Execute("var res = opentype.load();");

			//engine.AddModule("opentype", js);
			//var res = engine.Execute("opentype.load(), {}, {isUrl: true}");

			//var ns = engine.ImportModule("opentype");
			//var value = ns.Get("value").AsString();

			return (0f, 0f);
		}
	}
}
