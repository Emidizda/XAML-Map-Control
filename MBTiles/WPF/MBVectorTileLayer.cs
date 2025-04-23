using System;
using System.Diagnostics;
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
            get => (string)GetValue(StyleProperty);
            set => SetValue(StyleProperty, value);
        }

        protected virtual async Task<MBVectorTileSource> CreateTileSourceAsync(string file, string stylePath)
        {
            var tileSource = new MBVectorTileSource();

            await tileSource.OpenAsync(file, stylePath);


            return tileSource;
        }

        private async Task StylePathPropertyChanged(string file)
        {
            if (!string.IsNullOrEmpty(File) && !string.IsNullOrEmpty(StylePath))
            {
                await ConnectToDatabase(File, StylePath);
            }
        }

        private async Task FilePropertyChanged(string file)
        {
            if (!string.IsNullOrEmpty(File) && !string.IsNullOrEmpty(StylePath))
            {
                await ConnectToDatabase(File, StylePath);
            }
        }

        private async Task ConnectToDatabase(string filePath, string stylePath)
        {
            (TileSource as MBVectorTileSource)?.Close();

            ClearValue(TileSourceProperty);
            ClearValue(SourceNameProperty);
            ClearValue(DescriptionProperty);
            ClearValue(MinZoomLevelProperty);
            ClearValue(MaxZoomLevelProperty);

            if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(stylePath))
            {
                try
                {
                    var tileSource = await CreateTileSourceAsync(filePath, stylePath);

                    TileSource = tileSource;

                    if (tileSource.Metadata.TryGetValue("name", out string value))
                    {
                        SourceName = value;
                    }

                    if (tileSource.Metadata.TryGetValue("description", out value))
                    {
                        Description = value;
                    }

                    if (tileSource.Metadata.TryGetValue("minzoom", out value) && int.TryParse(value, out int zoomLevel))
                    {
                        MinZoomLevel = zoomLevel;
                    }

                    if (tileSource.Metadata.TryGetValue("maxzoom", out value) && int.TryParse(value, out zoomLevel))
                    {
                        MaxZoomLevel = zoomLevel;
                    }

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to get out map information: " + e);
                }
            }
            

         
        }

    }
}