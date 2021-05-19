﻿using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
	public class Line : ILine
	{
		public Line()
		{

		}

		public Line(double x1, double y1, double x2, double y2)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}

		public double X1 { get; set; }

		public double Y1 { get; set; }

		public double X2 { get; set; }

		public double Y2 { get; set; }
	}
}