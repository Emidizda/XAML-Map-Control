using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapControl.MBTiles
{
    public class MBVectorTileSource : TileSource, IDisposable
    {

        //public MBVectorTileSource(string path, string stylePath, string cachePath)
        //{
            
        //}



        public void Close()
        {

        }

        public override async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            ImageSource image = null;

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