using SkiaSharp;
using System.Collections.Concurrent;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Clipper2Lib;

namespace VectorTileRenderer
{
    public class SkiaCanvas : ICanvas
    {
        int _width;
        int _height;

        WriteableBitmap _bitmap;
        SKSurface _surface;
        SKCanvas _canvas;
        public bool ClipOverflow { get; set; } = false;
        private Rect _clipRectangle;
        //List<IntPoint> clipRectanglePath;
        private List<Path64> _clipRectanglePath;

        ConcurrentDictionary<string, SKTypeface> _fontPairs = new ConcurrentDictionary<string, SKTypeface>();
        private static readonly Object FontLock = new Object();

        List<Rect> _textRectangles = new List<Rect>();

        public void StartDrawing(double width, double height)
        {
            this._width = (int)width;
            this._height = (int)height;

            _bitmap = new WriteableBitmap(this._width, this._height, 96, 96, PixelFormats.Pbgra32, null);
            _bitmap.Lock();
            var info = new SKImageInfo(this._width, this._height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

            //var glInterface = GRGlInterface.CreateNativeGlInterface();
            //grContext = GRContext.Create(GRBackend.OpenGL, glInterface);

            //renderTarget = SkiaGL.CreateRenderTarget();
            //renderTarget.Width = this.width;
            //renderTarget.Height = this.height;


            _surface = SKSurface.Create(info, _bitmap.BackBuffer, _bitmap.BackBufferStride);
            //surface = SKSurface.Create(grContext, renderTarget);
            _canvas = _surface.Canvas;

//TODO verify this somehow? from clipperLib to clipper2Lib
            //double padding = -5;
            //clipRectangle = new Rect(padding, padding, this.width - padding * 2, this.height - padding * 2);

            //clipRectanglePath = new List<IntPoint>();
            //clipRectanglePath.Add(new IntPoint((int)clipRectangle.Top, (int)clipRectangle.Left));
            //clipRectanglePath.Add(new IntPoint((int)clipRectangle.Top, (int)clipRectangle.Right));
            //clipRectanglePath.Add(new IntPoint((int)clipRectangle.Bottom, (int)clipRectangle.Right));
            //clipRectanglePath.Add(new IntPoint((int)clipRectangle.Bottom, (int)clipRectangle.Left));

            double padding = -5;
            Rect64 clipRectangle = new Rect64((long)padding, (long)padding, (long)(this._width - padding * 2), (long)(this._height - padding * 2));


            _clipRectanglePath = new List<Path64>();

            Path64 clipRectanglePath = new Path64
            {
                new Point64(clipRectangle.left, clipRectangle.top),
                new Point64(clipRectangle.right, clipRectangle.top),
                new Point64(clipRectangle.right, clipRectangle.bottom),
                new Point64(clipRectangle.left, clipRectangle.bottom)
            };
            _clipRectanglePath.Add(clipRectanglePath);


            //clipRectanglePath = new List<IntPoint>();
            //clipRectanglePath.Add(new IntPoint((int)clipRectangle.Top + 10, (int)clipRectangle.Left + 10));
            //clipRectanglePath.Add(new IntPoint((int)clipRectangle.Top + 10, (int)clipRectangle.Right - 10));
            //clipRectanglePath.Add(new IntPoint((int)clipRectangle.Bottom - 10, (int)clipRectangle.Right - 10));
            //clipRectanglePath.Add(new IntPoint((int)clipRectangle.Bottom - 10, (int)clipRectangle.Left + 10));
        }

        public void DrawBackground(Brush style)
        {
            _canvas.Clear(new SKColor(style.Paint.BackgroundColor.R, style.Paint.BackgroundColor.G, style.Paint.BackgroundColor.B, style.Paint.BackgroundColor.A));
        }


        //public void DrawPolygon(List geometry, Brush style)
        //{
        //    throw new NotImplementedException();
        //}

        public void DrawLineString(List geometry, Brush style)
        {
            

        }

        SKStrokeCap convertCap(PenLineCap cap)
        {
            if (cap == PenLineCap.Flat)
            {
                return SKStrokeCap.Butt;
            }
            else if (cap == PenLineCap.Round)
            {
                return SKStrokeCap.Round;
            }

            return SKStrokeCap.Square;
        }

        //private double getAngle(double x1, double y1, double x2, double y2)
        //{
        //    double degrees;

        //    if (x2 - x1 == 0)
        //    {
        //        if (y2 > y1)
        //            degrees = 90;
        //        else
        //            degrees = 270;
        //    }
        //    else
        //    {
        //        // Calculate angle from offset.
        //        double riseoverrun = (y2 - y1) / (x2 - x1);
        //        double radians = Math.Atan(riseoverrun);
        //        degrees = radians * (180 / Math.PI);

        //        if ((x2 - x1) < 0 || (y2 - y1) < 0)
        //            degrees += 180;
        //        if ((x2 - x1) > 0 && (y2 - y1) < 0)
        //            degrees -= 180;
        //        if (degrees < 0)
        //            degrees += 360;
        //    }
        //    return degrees;
        //}

        //private double getAngleAverage(double a, double b)
        //{
        //    a = a % 360;
        //    b = b % 360;

        //    double sum = a + b;
        //    if (sum > 360 && sum < 540)
        //    {
        //        sum = sum % 180;
        //    }
        //    return sum / 2;
        //}

        double Clamp(double number, double min = 0, double max = 1)
        {
            return Math.Max(min, Math.Min(max, number));
        }
        //TODO this was the old ClipperLib method
        //List<List<Point>> clipPolygon(List<Point> geometry) // may break polygons into multiple ones
        //{
        //    Clipper c = new Clipper();

        //    var polygon = new List<IntPoint>();

        //    foreach (var point in geometry)
        //    {
        //        polygon.Add(new IntPoint((int)point.X, (int)point.Y));
        //    }

        //    c.AddPolygon(polygon, PolyType.ptSubject);

        //    c.AddPolygon(clipRectanglePath, PolyType.ptClip);

        //    List<List<IntPoint>> solution = new List<List<IntPoint>>();

        //    bool success = c.Execute(ClipType.ctIntersection, solution, PolyFillType.pftNonZero, PolyFillType.pftEvenOdd);

        //    if (success && solution.Count > 0)
        //    {
        //        var result = solution.Select(s => s.Select(item => new Point(item.X, item.Y)).ToList()).ToList();
        //        return result;
        //    }

        //    return null;
        //}

        List<List<Point>> ClipPolygon(List<Point> geometry)
        {
            Clipper64 clipper = new Clipper64();

            // Convert input polygon to Clipper2Lib's Path64 format
            Path64 subjectPolygon = new Path64();
            foreach (var point in geometry)
            {
                subjectPolygon.Add(new Point64((long)point.X, (long)point.Y));
            }
           
            // Add subject and clip polygons
            clipper.AddSubject(subjectPolygon);

            foreach (Path64 path64 in _clipRectanglePath)
            {
                clipper.AddClip(path64);
            }
          

            // Store the result
            Paths64 solution = new Paths64();

            // Perform intersection clipping
            bool success = clipper.Execute(ClipType.Intersection, Clipper2Lib.FillRule.NonZero, solution);

            // Convert back to List<List<Point>> format
            if (success && solution.Count > 0)
            {
                return solution.Select(poly => poly.Select(pt => new Point((int)pt.X, (int)pt.Y)).ToList()).ToList();
            }

            return null;
        }

        List<Point> ClipLine(List<Point> geometry)
        {
            return LineClipper.ClipPolyline(geometry, _clipRectangle);
        }

        SKPath GetPathFromGeometry(List<Point> geometry)
        {

            SKPath path = new SKPath
            {
                FillType = SKPathFillType.EvenOdd,
            };

            var firstPoint = geometry[0];

            path.MoveTo((float)firstPoint.X, (float)firstPoint.Y);
            foreach (var point in geometry.Skip(1))
            {
                var lastPoint = path.LastPoint;
                path.LineTo((float)point.X, (float)point.Y);
            }

            return path;
        }

        public void DrawLineString(List<Point> geometry, Brush style)
        {
            if (ClipOverflow)
            {
                geometry = ClipLine(geometry);
                if (geometry == null)
                {
                    return;
                }
            }

            var path = GetPathFromGeometry(geometry);
            if (path == null)
            {
                return;
            }

            SKPaint fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = convertCap(style.Paint.LineCap),
                StrokeWidth = (float)style.Paint.LineWidth,
                Color = new SKColor(style.Paint.LineColor.R, style.Paint.LineColor.G, style.Paint.LineColor.B, (byte)Clamp(style.Paint.LineColor.A * style.Paint.LineOpacity, 0, 255)),
                IsAntialias = true,
            };

            if (style.Paint.LineDashArray.Count() > 0)
            {
                var effect = SKPathEffect.CreateDash(style.Paint.LineDashArray.Select(n => (float)n).ToArray(), 0);
                fillPaint.PathEffect = effect;
            }

            //Debug.WriteLine("CANVAS LINE WIDTH: " + style.Paint.LineWidth);
            //Debug.WriteLine("COLOR: " + fillPaint.Color);

            _canvas.DrawPath(path, fillPaint);
        }

        SKTextAlign ConvertAlignment(TextAlignment alignment)
        {
            if (alignment == TextAlignment.Center)
            {
                return SKTextAlign.Center;
            }
            else if (alignment == TextAlignment.Left)
            {
                return SKTextAlign.Left;
            }
            else if (alignment == TextAlignment.Right)
            {
                return SKTextAlign.Right;
            }

            return SKTextAlign.Center;
        }

        SKPaint GetTextStrokePaint(Brush style)
        {
            var paint = new SKPaint()
            {
                IsStroke = true,
                StrokeWidth = (float)style.Paint.TextStrokeWidth,
                Color = new SKColor(style.Paint.TextStrokeColor.R, style.Paint.TextStrokeColor.G, style.Paint.TextStrokeColor.B, (byte)Clamp(style.Paint.TextStrokeColor.A * style.Paint.TextOpacity, 0, 255)),
                TextSize = (float)style.Paint.TextSize,
                IsAntialias = true,
                TextEncoding = SKTextEncoding.Utf32,
                TextAlign = ConvertAlignment(style.Paint.TextJustify),
                Typeface = getFont(style.Paint.TextFont, style),
            };

            return paint;
        }

        SKPaint GetTextPaint(Brush style)
        {
            var paint = new SKPaint()
            {
                Color = new SKColor(style.Paint.TextColor.R, style.Paint.TextColor.G, style.Paint.TextColor.B, (byte)Clamp(style.Paint.TextColor.A * style.Paint.TextOpacity, 0, 255)),
                IsAntialias = true,
            };

            return paint;
        }

        SKFont GetFontFromStyle(Brush style)
        {
            var font = new SKFont()
            {
                Size = (float)style.Paint.TextSize,
                Typeface = getFont(style.Paint.TextFont, style),
                Hinting = SKFontHinting.Normal,

            };
            return font; 
        }

        private string TransformText(string text, Brush style)
        {
            if (text.Length == 0)
            {
                return "";
            }

            if (style.Paint.TextTransform == TextTransform.Uppercase)
            {
                text = text.ToUpper();
            }
            else if (style.Paint.TextTransform == TextTransform.Lowercase)
            {
                text = text.ToLower();
            }

            var paint = GetTextPaint(style);
            text = BreakText(text, paint, style);

            return text;
            //return Encoding.UTF32.GetBytes(newText);
        }

        private string BreakText(string input, SKPaint paint, Brush style)
        {
            var restOfText = input;
            var brokenText = "";
            do
            {
                var lineLength = paint.BreakText(restOfText, (float)(style.Paint.TextMaxWidth * style.Paint.TextSize));

                if (lineLength == restOfText.Length)
                {
                    // its the end
                    brokenText += restOfText.Trim();
                    break;
                }

                var lastSpaceIndex = restOfText.LastIndexOf(' ', (int)(lineLength - 1));
                if (lastSpaceIndex == -1 || lastSpaceIndex == 0)
                {
                    // no more spaces, probably ;)
                    brokenText += restOfText.Trim();
                    break;
                }

                brokenText += restOfText.Substring(0, (int)lastSpaceIndex).Trim() + "\n";

                restOfText = restOfText.Substring((int)lastSpaceIndex, restOfText.Length - (int)lastSpaceIndex);

            } while (restOfText.Length > 0);

            return brokenText.Trim();
        }

        bool TextCollides(Rect rectangle)
        {
            foreach (var rect in _textRectangles)
            {
                if (rect.IntersectsWith(rectangle))
                {
                    return true;
                }
            }
            return false;
        }

        SKTypeface getFont(string[] familyNames, Brush style)
        {
            lock (FontLock)
            {
                foreach (var name in familyNames)
                {
                    if (_fontPairs.ContainsKey(name))
                    {
                        return _fontPairs[name];
                    }

                    if (style.GlyphsDirectory != null)
                    {
                        // check file system for ttf
                        var newType = SKTypeface.FromFile(System.IO.Path.Combine(style.GlyphsDirectory, name + ".ttf"));
                        if (newType != null)
                        {
                            _fontPairs[name] = newType;
                            return newType;
                        }

                        // check file system for otf
                        newType = SKTypeface.FromFile(System.IO.Path.Combine(style.GlyphsDirectory, name + ".otf"));
                        if (newType != null)
                        {
                            _fontPairs[name] = newType;
                            return newType;
                        }
                    }

                    var typeface = SKTypeface.FromFamilyName(name);
                    if (typeface.FamilyName == name)
                    {
                        // gotcha!
                        _fontPairs[name] = typeface;
                        return typeface;
                    }
                }

                // all options exhausted...
                // get the first one
                var fallback = SKTypeface.FromFamilyName(familyNames.First());
                _fontPairs[familyNames.First()] = fallback;
                return fallback;
            }
        }

        SKTypeface qualifyTypeface(string text, SKTypeface typeface)
        {
            var glyphs = new ushort[typeface.CountGlyphs(text)];
            if (glyphs.Length < text.Length)
            {
                var fm = SKFontManager.Default;
                var charIdx = (glyphs.Length > 0) ? glyphs.Length : 0;
                return fm.MatchCharacter(text[glyphs.Length]);
            }

            return typeface;
        }

        void QualifyTypeface(Brush style, SKFont paint)
        {
            var glyphs = new ushort[paint.Typeface.CountGlyphs(style.Text)];
            if (glyphs.Length < style.Text.Length)
            {
                var fm = SKFontManager.Default;
                var charIdx = (glyphs.Length > 0) ? glyphs.Length : 0;
                var newTypeface = fm.MatchCharacter(style.Text[glyphs.Length]);

                if (newTypeface == null)
                {
                    return;
                }

                paint.Typeface = newTypeface;

                glyphs = new ushort[newTypeface.CountGlyphs(style.Text)];
                if (glyphs.Length < style.Text.Length)
                {
                    // still causing issues
                    // so we cut the rest
                    charIdx = (glyphs.Length > 0) ? glyphs.Length : 0;

                    style.Text = style.Text.Substring(0, charIdx);
                }
            }

        }

        public void DrawText(Point geometry, Brush style)
        {
            if (style.Paint.TextOptional)
            {
                // TODO check symbol collision
                //return;
            }

            var paint = GetTextPaint(style);
            var font = GetFontFromStyle(style);
            QualifyTypeface(style, font);

           // var strokePaint = GetTextStrokePaint(style);
            var text = TransformText(style.Text, style);
            var allLines = text.Split('\n');

            //paint.Typeface = qualifyTypeface(text, paint.Typeface);

            // detect collisions
            if (allLines.Length > 0)
            {
                var biggestLine = allLines.OrderBy(line => line.Length).Last();
                
                var width = (int)(font.MeasureText(biggestLine));
                int left = (int)(geometry.X - width / 2);
                int top = (int)(geometry.Y - style.Paint.TextSize / 2 * allLines.Length);
                int height = (int)(style.Paint.TextSize * allLines.Length);

                var rectangle = new Rect(left, top, width, height);
                rectangle.Inflate(5, 5);

                if (ClipOverflow)
                {
                    if (!_clipRectangle.Contains(rectangle))
                    {
                        return;
                    }
                }

                if (TextCollides(rectangle))
                {
                    // collision detected
                    return;
                }
                _textRectangles.Add(rectangle);

                //var list = new List<Point>()
                //{
                //    rectangle.TopLeft,
                //    rectangle.TopRight,
                //    rectangle.BottomRight,
                //    rectangle.BottomLeft,
                //};

                //var brush = new Brush();
                //brush.Paint = new Paint();
                //brush.Paint.FillColor = Color.FromArgb(150, 255, 0, 0);

                //this.DrawPolygon(list, brush);
            }

            int i = 0;
            foreach (var line in allLines)
            {
                var textToDraw =line;
                float lineOffset = (float)(i * style.Paint.TextSize) - ((float)(allLines.Length) * (float)style.Paint.TextSize) / 2 + (float)style.Paint.TextSize;
                var position = new SKPoint((float)geometry.X + (float)(style.Paint.TextOffset.X * style.Paint.TextSize), (float)geometry.Y + (float)(style.Paint.TextOffset.Y * style.Paint.TextSize) + lineOffset);
                if (style.Paint.TextStrokeWidth != 0)
                {
                   // canvas.DrawText(bytes, position, strokePaint);
                    _canvas.DrawText(textToDraw, position, SKTextAlign.Center, font, paint);
                }

                _canvas.DrawText(textToDraw, position, SKTextAlign.Center,font, paint);
                i++;
            }

        }

        double getPathLength(List<Point> path)
        {
            double distance = 0;
            for (var i = 0; i < path.Count - 2; i++)
            {
                distance += (path[i] - path[i + 1]).Length;
            }

            return distance;
        }

        double getAbsoluteDiff2Angles(double x, double y, double c = Math.PI)
        {
            return c - Math.Abs((Math.Abs(x - y) % 2 * c) - c);
        }

        bool CheckPathSqueezing(List<Point> path, double textHeight)
        {
            //double maxCurve = 0;
            double previousAngle = 0;
            for (var i = 0; i < path.Count - 2; i++)
            {
                var vector = (path[i] - path[i + 1]);

                var angle = Math.Atan2(vector.Y, vector.X);
                var angleDiff = Math.Abs(getAbsoluteDiff2Angles(angle, previousAngle));

                //var length = vector.Length / textHeight;
                //var curve = angleDiff / length;
                //maxCurve = Math.Max(curve, maxCurve);


                if (angleDiff > Math.PI / 3)
                {
                    return true;
                }

                previousAngle = angle;
            }

            return false;

            //return 0;

            //return maxCurve;
        }

        void debugRectangle(Rect rectangle, Color color)
        {
            var list = new List<Point>()
            {
                rectangle.TopLeft,
                rectangle.TopRight,
                rectangle.BottomRight,
                rectangle.BottomLeft,
            };

            var brush = new Brush();
            brush.Paint = new Paint();
            brush.Paint.FillColor = color;

            this.DrawPolygon(list, brush);
        }

        public void DrawTextOnPath(List<Point> geometry, Brush style)
        {
            // buggggyyyyyy
            // requires an amazing collision system to work :/
            // --
            //return;

            //if (ClipOverflow)
            //{
            geometry = ClipLine(geometry);
            if (geometry == null)
            {
                return;
            }
            //}

            var path = GetPathFromGeometry(geometry);
            var text = TransformText(style.Text, style);

            var pathSqueezed = CheckPathSqueezing(geometry, style.Paint.TextSize);

            if (pathSqueezed)
            {
                return;
            }

            //text += " : " + bending.ToString("F");

            var bounds = path.Bounds;

            var left = bounds.Left - style.Paint.TextSize;
            var top = bounds.Top - style.Paint.TextSize;
            var right = bounds.Right + style.Paint.TextSize;
            var bottom = bounds.Bottom + style.Paint.TextSize;

            var rectangle = new Rect(left, top, right - left, bottom - top);

            //if (rectangle.Left <= 0 || rectangle.Right >= width || rectangle.Top <= 0 || rectangle.Bottom >= height)
            //{
            //    debugRectangle(rectangle, Color.FromArgb(128, 255, 100, 100));
            //    // bounding box (much bigger) collides with edges
            //    return;
            //}

            if (TextCollides(rectangle))
            {
                //debugRectangle(rectangle, Color.FromArgb(128, 100, 255, 100));
                // collides with other
                return;
            }
            _textRectangles.Add(rectangle);

            if (style.Text.Length * style.Paint.TextSize * 0.2 >= getPathLength(geometry))
            {
                //debugRectangle(rectangle, Color.FromArgb(128, 100, 100, 255));
                // exceeds estimated path length
                return;
            }


            //debugRectangle(rectangle, Color.FromArgb(150, 255, 0, 0));



            var offset = new SKPoint((float)style.Paint.TextOffset.X, (float)style.Paint.TextOffset.Y);
            //var bytes = Encoding.UTF32.GetBytes(text).ToString();
            if (style.Paint.TextStrokeWidth != 0)
            {
                // TODO implement this func custom way...
                //_canvas.DrawTextOnPath(bytes, path, offset, GetTextStrokePaint(style));
                _canvas.DrawTextOnPath(text, path, offset, GetFontFromStyle(style), GetTextStrokePaint(style));
            }

            //_canvas.DrawTextOnPath(bytes, path, offset, GetTextPaint(style));
            _canvas.DrawTextOnPath(text, path, offset, GetFontFromStyle(style), GetTextStrokePaint(style));


            //canvas.DrawText(Encoding.UTF32.GetBytes(bending.ToString("F")), new SKPoint((float)left + 10, (float)top + 10), getTextStrokePaint(style));
            //canvas.DrawText(Encoding.UTF32.GetBytes(bending.ToString("F")), new SKPoint((float)left + 10, (float)top + 10), getTextPaint(style));
        }

        public void DrawPoint(Point geometry, Brush style)
        {
            if (style.Paint.IconImage != null)
            {
                // draw icon here
            }
        }

        public void DrawPolygon(List<Point> geometry, Brush style)
        {
            List<List<Point>> allGeometries = null;
            if (ClipOverflow)
            {
                allGeometries = ClipPolygon(geometry);
            }
            else
            {
                allGeometries = new List<List<Point>>() { geometry };
            }

            if (allGeometries == null)
            {
                return;
            }

            foreach (var geometryPart in allGeometries)
            {
                var path = GetPathFromGeometry(geometryPart);
                if (path == null)
                {
                    return;
                }

                SKPaint fillPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    StrokeCap = convertCap(style.Paint.LineCap),
                    Color = new SKColor(style.Paint.FillColor.R, style.Paint.FillColor.G, style.Paint.FillColor.B, (byte)Clamp(style.Paint.FillColor.A * style.Paint.FillOpacity, 0, 255)),
                    IsAntialias = true,
                };

                _canvas.DrawPath(path, fillPaint);
            }

        }


        static SKImage ToSkImage(BitmapSource bitmap)
        {
            // TODO: maybe keep the same color types where we can, instead of just going to the platform default
            var info = new SKImageInfo(bitmap.PixelWidth, bitmap.PixelHeight);
            var image = SKImage.Create(info);
            using (var pixmap = image.PeekPixels())
            {
                ToSkPixmap(bitmap, pixmap);
            }
            return image;
        }

        static void ToSkPixmap(BitmapSource bitmap, SKPixmap pixmap)
        {
            // TODO: maybe keep the same color types where we can, instead of just going to the platform default
            if (pixmap.ColorType == SKImageInfo.PlatformColorType)
            {
                var info = pixmap.Info;
                var converted = new FormatConvertedBitmap(bitmap, PixelFormats.Pbgra32, null, 0);
                converted.CopyPixels(new Int32Rect(0, 0, info.Width, info.Height), pixmap.GetPixels(), info.BytesSize, info.RowBytes);
            }
            else
            {
                // we have to copy the pixels into a format that we understand
                // and then into a desired format
                // TODO: we can still do a bit more for other cases where the color types are the same
                using (var tempImage = ToSkImage(bitmap))
                {
                    tempImage.ReadPixels(pixmap, 0, 0);
                }
            }
        }


        public void DrawTextOnPath(List geometry, Brush style)
        {
           
        }

        public void DrawImage(Stream imageStream, Brush style)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = imageStream;
            bitmapImage.DecodePixelWidth = this._width;
            bitmapImage.DecodePixelHeight = this._height;
            bitmapImage.EndInit();

            var image = ToSkImage(bitmapImage);

            _canvas.DrawImage(image, new SKPoint(0, 0));
        }

        public void DrawUnknown(List geometry, Brush style)
        {
           
        }

        public void DrawUnknown(List<List<Point>> geometry, Brush style)
        {

        }

        public BitmapSource FinishDrawing()
        {
            //using (var paint = new SKPaint())
            //{
            //    paint.Color = new SKColor(255, 255, 255, 255);
            //    paint.Style = SKPaintStyle.Fill;
            //    paint.TextSize = 24;
            //    paint.IsAntialias = true;

            //    var bytes = Encoding.UTF32.GetBytes("HELLO WORLD");
            //    canvas.DrawText(bytes, new SKPoint(10, 10), paint);
            //}


            //surface.Canvas.Flush();
            //grContext.


            _bitmap.AddDirtyRect(new Int32Rect(0, 0, this._width, this._height));
            _bitmap.Unlock();
            _bitmap.Freeze();

            return _bitmap;

        }
    }
}


