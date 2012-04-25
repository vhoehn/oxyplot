// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SvgRenderContext.cs" company="OxyPlot">
//   http://oxyplot.codeplex.com, license: Ms-PL
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The svg render context.
    /// </summary>
    public class SvgRenderContext : RenderContextBase, IDisposable
    {
        #region Constants and Fields

        /// <summary>
        ///   The svg writer.
        /// </summary>
        private readonly SvgWriter w;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgRenderContext"/> class.
        /// </summary>
        /// <param name="s">
        /// The s.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <param name="isDocument">
        /// The is document.
        /// </param>
        public SvgRenderContext(Stream s, double width, double height, bool isDocument)
        {
            this.w = new SvgWriter(s, width, height, isDocument);
            this.Width = width;
            this.Height = height;
            this.PaintBackground = true;
        }

#if !METRO

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgRenderContext"/> class.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        public SvgRenderContext(string path, double width, double height)
        {
            this.w = new SvgWriter(path, width, height);
            this.Width = width;
            this.Height = height;
        }

#endif

        #endregion

        #region Public Methods

        /// <summary>
        /// Closes the svg writer.
        /// </summary>
        public void Close()
        {
            this.w.Close();
        }

        /// <summary>
        /// Completes the svg element.
        /// </summary>
        public void Complete()
        {
            this.w.Complete();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.w.Dispose();
        }

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="fill">The fill color.</param>
        /// <param name="stroke">The stroke color.</param>
        /// <param name="thickness">The thickness.</param>
        public override void DrawEllipse(OxyRect rect, OxyColor fill, OxyColor stroke, double thickness)
        {
            this.w.WriteEllipse(
                rect.Left, rect.Top, rect.Width, rect.Height, this.w.CreateStyle(fill, stroke, thickness, null));
        }

        /// <summary>
        /// Draws the polyline from the specified points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="stroke">The stroke color.</param>
        /// <param name="thickness">The stroke thickness.</param>
        /// <param name="dashArray">The dash array.</param>
        /// <param name="lineJoin">The line join type.</param>
        /// <param name="aliased">if set to <c>true</c> the shape will be aliased.</param>
        public override void DrawLine(
            IList<ScreenPoint> points,
            OxyColor stroke,
            double thickness,
            double[] dashArray,
            OxyPenLineJoin lineJoin,
            bool aliased)
        {
            this.w.WritePolyline(points, this.w.CreateStyle(null, stroke, thickness, dashArray, lineJoin));
        }

        /// <summary>
        /// Draws the polygon from the specified points. The polygon can have stroke and/or fill.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="fill">The fill color.</param>
        /// <param name="stroke">The stroke color.</param>
        /// <param name="thickness">The stroke thickness.</param>
        /// <param name="dashArray">The dash array.</param>
        /// <param name="lineJoin">The line join type.</param>
        /// <param name="aliased">if set to <c>true</c> the shape will be aliased.</param>
        public override void DrawPolygon(
            IList<ScreenPoint> points,
            OxyColor fill,
            OxyColor stroke,
            double thickness,
            double[] dashArray,
            OxyPenLineJoin lineJoin,
            bool aliased)
        {
            this.w.WritePolygon(points, this.w.CreateStyle(fill, stroke, thickness, dashArray, lineJoin));
        }

        /// <summary>
        /// Draws the rectangle.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="fill">The fill color.</param>
        /// <param name="stroke">The stroke color.</param>
        /// <param name="thickness">The stroke thickness.</param>
        public override void DrawRectangle(OxyRect rect, OxyColor fill, OxyColor stroke, double thickness)
        {
            this.w.WriteRectangle(
                rect.Left, rect.Top, rect.Width, rect.Height, this.w.CreateStyle(fill, stroke, thickness, null));
        }

        /// <summary>
        /// Draws the text.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="text">The text.</param>
        /// <param name="c">The c.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="rotate">The rotate.</param>
        /// <param name="halign">The halign.</param>
        /// <param name="valign">The valign.</param>
        /// <param name="maxSize">Size of the max.</param>
        public override void DrawText(
            ScreenPoint p, 
            string text, 
            OxyColor c, 
            string fontFamily, 
            double fontSize, 
            double fontWeight, 
            double rotate, 
            HorizontalTextAlign halign, 
            VerticalTextAlign valign, 
            OxySize? maxSize)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var lines = Regex.Split(text, "\r\n");
            if (valign == VerticalTextAlign.Bottom)
            {
                for (var i = lines.Length - 1; i >= 0; i--)
                {
                    var line = lines[i];
                    var size = this.MeasureText(line, fontFamily, fontSize, fontWeight);
                    this.w.WriteText(p, line, c, fontFamily, fontSize, fontWeight, rotate, halign, valign);

                    p.X += Math.Sin(rotate / 180.0 * Math.PI) * size.Height;
                    p.Y -= Math.Cos(rotate / 180.0 * Math.PI) * size.Height;
                }
            }
            else
            {
                foreach (var line in lines)
                {
                    var size = this.MeasureText(line, fontFamily, fontSize, fontWeight);
                    this.w.WriteText(p, line, c, fontFamily, fontSize, fontWeight, rotate, halign, valign);

                    p.X -= Math.Sin(rotate / 180.0 * Math.PI) * size.Height;
                    p.Y += Math.Cos(rotate / 180.0 * Math.PI) * size.Height;
                }
            }
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public void Flush()
        {
            this.w.Flush();
        }

        /// <summary>
        /// Measures the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <returns>
        /// The text size.
        /// </returns>
        public override OxySize MeasureText(string text, string fontFamily, double fontSize, double fontWeight)
        {
            if (string.IsNullOrEmpty(text))
            {
                return OxySize.Empty;
            }

            // todo: should improve text measuring, currently using p/invoke on GDI32
            // is it better to use winforms or wpf text measuring?
            return Gdi32.MeasureString(fontFamily, (int)fontSize, (int)fontWeight, text);
        }

        #endregion
    }
}