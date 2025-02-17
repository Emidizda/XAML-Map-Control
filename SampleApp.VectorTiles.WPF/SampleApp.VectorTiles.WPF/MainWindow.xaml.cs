using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VectorTileRenderer;

namespace SampleApp.VectorTiles.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        GlobalMercator gmt = new GlobalMercator();
        //string mainDir = "../../../";
        string mainDir = "C:/GitRepos/VectorMaps/XAML-Map-Control/";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Zurich_mbtiles(object sender, RoutedEventArgs e)
        {
            zurichMbTilesAliFluxStyle();
           // showMbTiles(mainDir + @"tiles/islamabad.mbtiles", mainDir + @"styles/bright-style.json", 1438, 1226, 1440, 1228, 11, 512);
            //showPbf(mainDir + @"tiles/islamabad.pbf", mainDir + @"styles/basic-style.json", 11, 512, 2);
            //  showPbf(mainDir + @"tiles/zurich.pbf.gz", mainDir + @"styles/basic-style.json", 14);
        }

        void zurichMbTilesAliFluxStyle()
        {
            showMbTiles(mainDir + @"tiles/zurich.mbtiles", mainDir + @"styles/aliflux-style.json", 8579, 10645, 8581, 10647, 14, 512);
        }

        async void showMbTiles(string path, string stylePath, int minX, int minY, int maxX, int maxY, int zoom, double size = 512, double scale = 1)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // load style and font
            var style = new VectorTileRenderer.Style(stylePath);
            style.FontDirectory = mainDir + @"styles/fonts/";

            // set pbf as tile provider
            var provider = new VectorTileRenderer.Sources.MbTilesSource(path);
            style.SetSourceProvider(0, provider);

            Debug.WriteLine("MaxX: " + maxX + "  - MinX: " + minX);
            Debug.WriteLine("MaxY: " + maxY + "  - MinY: " + minY);

            BitmapSource[,] bitmapSources = new BitmapSource[maxX - minX + 1, maxY - minY + 1];

            // loop through tiles and render them
            Parallel.For(minX, maxX + 1, (int x) =>
            {
                Parallel.For(minY, maxY + 1, async void (int y) =>
                {
                    try
                    {
                        var canvas = new SkiaCanvas();
                        var bitmapR = await Renderer.Render(style, canvas, x, y, zoom, size, size, scale);

                        if (bitmapR == null)
                        {
                            Debug.WriteLine("bitmapR is null");
                        }

                        bitmapSources[x - minX, maxY - y] = bitmapR;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Render loop failed async failed!");
                    }
                });
            });

            // merge the tiles and show it
            if (bitmapSources[0,0] != null)
            {
                var bitmap = mergeBitmaps(bitmapSources);
                demoImage.Source = bitmap;
            }
            

            scrollViewer.Background = new SolidColorBrush(style.GetBackgroundColor(zoom));

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs + "ms time");
        }

        BitmapSource mergeBitmaps(BitmapSource[,] bitmapSources)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                for (int x = 0; x < bitmapSources.GetLength(0); x++)
                {
                    for (int y = 0; y < bitmapSources.GetLength(1); y++)
                    {
                        drawingContext.DrawImage(bitmapSources[x, y], new Rect(x * bitmapSources[x, y].Width, y * bitmapSources[x, y].Height, bitmapSources[x, y].Width, bitmapSources[x, y].Height));
                    }
                }
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(bitmapSources.GetLength(0) * bitmapSources[0, 0].Width), (int)(bitmapSources.GetLength(1) * bitmapSources[0, 0].Height), 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            bmp.Freeze();

            return bmp;
        }

        private void Ot_mbTiles(object sender, RoutedEventArgs e)
        {
            var coords = gmt.LatLonToTile(47.382047, 8.525868, 12);
            showMbTiles(mainDir + @"tiles/worldOT.mbtiles", mainDir + @"styles/basic-style.json", 8579, 10645, 8581, 10647, 14, 512);
        }

        private void Zuric_pbf(object sender, RoutedEventArgs e)
        {
            showPbf(mainDir + @"tiles/zurich.pbf", mainDir + @"styles/basic-style.json", 14);
        }

        async void showPbf(string path, string stylePath, double zoom, double size = 512, double scale = 1)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // load style and font
            var style = new VectorTileRenderer.Style(stylePath);
            style.FontDirectory = mainDir + @"styles/fonts/";

            // set pbf as tile provider
            var provider = new VectorTileRenderer.Sources.PbfTileSource(path);
            style.SetSourceProvider(0, provider);

            // render it on a skia canvas
            var canvas = new SkiaCanvas();
            var bitmapR = await Renderer.Render(style, canvas, 0, 0, zoom, size, size, scale);
            demoImage.Source = bitmapR;

            scrollViewer.Background = new SolidColorBrush(style.GetBackgroundColor(zoom));

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs + "ms time");
        }
    }
}