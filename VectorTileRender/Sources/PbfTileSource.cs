﻿//using Mapbox.VectorTile.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using Mapbox.Vector.Tile;

namespace VectorTileRenderer.Sources
{
    public class PbfTileSource : IVectorTileSource
    {
        public string Path { get; set; } = "";
        public Stream Stream { get; set; } = null;

        public PbfTileSource(string path)
        {
            this.Path = path;
        }

        public PbfTileSource(Stream stream)
        {
            this.Stream = stream;
        }

        public async Task<Stream> GetTile(int x, int y, int zoom)
        {
            var qualifiedPath = Path
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoom.ToString());
            return File.Open(qualifiedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        
        public async Task<VectorTile> GetVectorTile(int x, int y, int zoom)
        {
            if(Path != "")
            {
                using (var stream = await GetTile(x, y, zoom))
                {
                    return await unzipStream(stream);
                }
            } else if (Stream != null)
            {
                return  await unzipStream(Stream);
            }

            return null;
        }

        private async Task<VectorTile> unzipStream(Stream stream)
        {
            if (isGZipped(stream))
            {
                using (var zipStream = new GZipStream(stream, CompressionMode.Decompress))
                using (var resultStream = new MemoryStream())
                {
                    zipStream.CopyTo(resultStream);
                    resultStream.Seek(0, SeekOrigin.Begin);
                    return await loadStream(resultStream);
                }
            }
            else
            {
                return await loadStream(stream);
            }
        }
        
        private async Task<VectorTile> loadStream(Stream stream)
        {
            //var mbLayers = new Mapbox.VectorTile.VectorTile(readTillEnd(stream));
           
            List<Mapbox.Vector.Tile.VectorTileLayer> vectorTileLayers = VectorTileParser.Parse(stream);

            return await baseTileToVector(vectorTileLayers);
        }

        static string convertGeometryType(Tile.GeomType type)
        {
            if (type == Tile.GeomType.LineString)
            {
                return "LineString";
            } else if (type == Tile.GeomType.Point)
            {
                return "Point";
            }
            else if (type == Tile.GeomType.Polygon)
            {
                return "Polygon";
            } else
            {
                return "Unknown";
            }
        }

        private static async Task<VectorTile> baseTileToVector(object baseTile)
        {
            //var tile = baseTile as Mapbox.VectorTile.VectorTile;
            if (baseTile is List<Mapbox.Vector.Tile.VectorTileLayer> layerInfos)
            {
                //var layerInfo = layerInfos[0];
                var result = new VectorTile();

                foreach (var layerInfo in layerInfos)
                {
                    Debug.WriteLine(layerInfo.Name);
                    var vectorLayer = new VectorTileLayer();
                    vectorLayer.Name = layerInfo.Name;

                    foreach (Mapbox.Vector.Tile.VectorTileFeature layerInfoVectorTileFeature in layerInfo.VectorTileFeatures)
                    {
                        var vectorFeature = new VectorTileFeature();
                        //TODO extent was 1 here?
                        vectorFeature.Extent = layerInfoVectorTileFeature.Extent;
                        vectorFeature.GeometryType = convertGeometryType(layerInfoVectorTileFeature.GeometryType);
                        vectorFeature.Attributes = layerInfoVectorTileFeature.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        var vectorGeometry = new List<List<Point>>();

                        foreach (List<Coordinate> coordinates in layerInfoVectorTileFeature.Geometry)
                        {
                            var vectorPoints = new List<Point>();
                            foreach (Coordinate coordinate in coordinates)
                            {
                                var dX = (double)coordinate.X / (double)vectorFeature.Extent;
                                var dY = (double)coordinate.Y / (double)vectorFeature.Extent;
                                vectorPoints.Add(new Point(dX, dY));
                            }
                            vectorGeometry.Add(vectorPoints);
                        }
                        vectorFeature.Geometry = vectorGeometry;
                        vectorLayer.Features.Add(vectorFeature);

                    }
                    result.Layers.Add(vectorLayer);
                }

                return result;

                //foreach (var lyrName in layerInfos)
                //{
                //    Mapbox.VectorTile.VectorTileLayer lyr = tile.GetLayer(lyrName);

                //    var vectorLayer = new VectorTileLayer();
                //    vectorLayer.Name = lyrName;

                //    for (int i = 0; i < lyr.FeatureCount(); i++)
                //    {
                //        Mapbox.VectorTile.VectorTileFeature feat = lyr.GetFeature(i);

                //        var vectorFeature = new VectorTileFeature();
                //        vectorFeature.Extent = 1;
                //        vectorFeature.GeometryType = convertGeometryType(feat.GeometryType);
                //        vectorFeature.Attributes = feat.GetProperties();

                //        var vectorGeometry = new List<List<Point>>();

                //        foreach (var points in feat.Geometry<int>())
                //        {
                //            var vectorPoints = new List<Point>();

                //            foreach (var coordinate in points)
                //            {
                //                var dX = (double)coordinate.X / (double)lyr.Extent;
                //                var dY = (double)coordinate.Y / (double)lyr.Extent;

                //                vectorPoints.Add(new Point(dX, dY));

                //                //var newX = Utils.ConvertRange(dX, extent.Left, extent.Right, 0, vectorFeature.Extent);
                //                //var newY = Utils.ConvertRange(dY, extent.Top, extent.Bottom, 0, vectorFeature.Extent);

                //                //vectorPoints.Add(new Point(newX, newY));
                //            }

                //            vectorGeometry.Add(vectorPoints);
                //        }

                //        vectorFeature.Geometry = vectorGeometry;
                //        vectorLayer.Features.Add(vectorFeature);
                //    }

                //    result.Layers.Add(vectorLayer);
                //}

                return result;
            }

            return null;

        }
        
        byte[] readTillEnd(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        bool isGZipped(Stream stream)
        {
            return isZipped(stream, 3, "1F-8B-08");
        }

        bool isZipped(Stream stream, int signatureSize = 4, string expectedSignature = "50-4B-03-04")
        {
            if (stream.Length < signatureSize)
                return false;
            byte[] signature = new byte[signatureSize];
            int bytesRequired = signatureSize;
            int index = 0;
            while (bytesRequired > 0)
            {
                int bytesRead = stream.Read(signature, index, bytesRequired);
                bytesRequired -= bytesRead;
                index += bytesRead;
            }
            stream.Seek(0, SeekOrigin.Begin);
            string actualSignature = BitConverter.ToString(signature);
            if (actualSignature == expectedSignature) return true;
            return false;
        }
    }
}
