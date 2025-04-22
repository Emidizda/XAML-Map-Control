using System.Threading.Tasks;
using System.Windows;

namespace MapControl.MBTiles
{
    public class MBVectorTileLayer : MapTileLayer
    {
        public static readonly DependencyProperty FileProperty =
            DependencyPropertyHelper.Register<MBVectorTileLayer, string>(nameof(File), null,
                async (layer, oldValue, newValue) => await layer.FilePropertyChanged(newValue));

        public string File
        {
            get => (string)GetValue(FileProperty);
            set => SetValue(FileProperty, value);
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyPropertyHelper.Register<MBVectorTileLayer, string>(nameof(StylePath), null,
                async (layer, oldValue, newValue) => await layer.StylePathPropertyChanged(newValue));

        public string StylePath
        {
            get;
            set;
        }

        protected virtual async Task<MBVectorTileSource> CreateTileSourceAsync(string file)
        {
            var tileSource = new MBVectorTileSource();


            return tileSource;
        }

        private async Task StylePathPropertyChanged(string file)
        {
            (TileSource as MBVectorTileSource)?.Close();

            ClearValue(TileSourceProperty);
            ClearValue(SourceNameProperty);
            ClearValue(DescriptionProperty);
            ClearValue(MinZoomLevelProperty);
            ClearValue(MaxZoomLevelProperty);



        }

        private async Task FilePropertyChanged(string file)
        {
            (TileSource as MBVectorTileSource)?.Close();

            ClearValue(TileSourceProperty);
            ClearValue(SourceNameProperty);
            ClearValue(DescriptionProperty);
            ClearValue(MinZoomLevelProperty);
            ClearValue(MaxZoomLevelProperty);
        }

    }
}