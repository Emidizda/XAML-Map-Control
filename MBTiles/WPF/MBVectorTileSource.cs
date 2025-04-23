using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using VectorTileRenderer;

namespace MapControl.MBTiles
{
    public class MBVectorTileSource : TileSource, IDisposable
    {
        Style style;
        VectorTileRenderer.Sources.MbTilesSource provider;
        string cachePath;
      
        public IDictionary<string, string> Metadata => provider.Metadata;

        public async Task OpenAsync(string file, string stylePath)
        {
            style = new Style(stylePath);
            style.FontDirectory = @"styles/fonts/";

            provider = new VectorTileRenderer.Sources.MbTilesSource();
            await provider.OpenAsync(file);
            style.SetSourceProvider("openmaptiles", provider);
        }


        public void Close()
        {

        }

        public override async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            ImageSource image = null;
            var canvas = new SkiaCanvas();
            try
            {
                var newY = (1 << zoomLevel) - y - 1;
                image = Renderer.Render(style, canvas, x, newY, zoomLevel, 256, 256, 1).Result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to render bitmap");
                return null;
            }

            return image;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Close();
            }
        }

         
    }
}