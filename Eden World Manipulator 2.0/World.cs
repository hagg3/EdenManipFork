using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Eden_World_Maniputor_2._0
{
    public class World
    {
        public string Name;
        public Dictionary<int, Point> Chunks;
        public Rectangle WorldArea = default(Rectangle);
        public static int skyColor;
        public static string worldName;
        public byte[] Bytes;

        public static World LoadWorld(string path)
        {
            return BuildWorld(File.ReadAllBytes(path));
        }

        public static World LoadDownload(byte[] down)
        {
            return BuildWorld(down);
        }

        private static World BuildWorld(byte[] bytes)
        {
            int[] skyColorHistogram = new int[256];

            for (int i = 132; i <= 148; i++)
            {
                byte skyByte = bytes[i];
                if (skyByte != 14)
                {
                    skyColorHistogram[skyByte]++;
                }
            }

            int mostCommonSkyColor = 14;
            int maxCount = 0;
            for (int i = 0; i < skyColorHistogram.Length; i++)
            {
                if (skyColorHistogram[i] > maxCount)
                {
                    mostCommonSkyColor = i;
                    maxCount = skyColorHistogram[i];
                }
            }
            skyColor = mostCommonSkyColor;

            int chunkPointerStartIndex = bytes[35] * 256 * 256 * 256 + bytes[34] * 256 * 256 + bytes[33] * 256 + bytes[32];

            int nameLength = 0;
            for (int i = 40; i <= 75; i++)
            {
                if (bytes[i] == 0)
                {
                    break;
                }

                nameLength++;
            }

            worldName = Encoding.ASCII.GetString(bytes, 40, nameLength);

            Dictionary<int, Point> chunks = new Dictionary<int, Point>();
            int currentChunkPointerIndex = chunkPointerStartIndex;
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            do
            {
                int address = bytes[currentChunkPointerIndex + 11] * 256 * 256 * 256 + bytes[currentChunkPointerIndex + 10] * 256 * 256 + bytes[currentChunkPointerIndex + 9] * 256 + bytes[currentChunkPointerIndex + 8];
                Point point = new Point(bytes[currentChunkPointerIndex + 1] * 256 + bytes[currentChunkPointerIndex], bytes[currentChunkPointerIndex + 5] * 256 + bytes[currentChunkPointerIndex + 4]);

                chunks.Add(address, point);

                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            } while ((currentChunkPointerIndex += 16) < bytes.Length);

            Rectangle worldArea = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

            return new World(bytes, chunks, worldName, worldArea);
        }

        public void SaveWorld(string path, World world)
        {
            using (FileStream stream = new FileStream(path, FileMode.CreateNew))
            {
                //Save File with changed world name or original world name
                byte[] name = Encoding.ASCII.GetBytes(worldName);
                for (int i = 0; i < world.Bytes.Length; i++)
                {
                    if (i >= 40 && i <= (75))
                    {
                        if (i - 40 < name.Length)
                        {
                            stream.WriteByte(name[i - 40]);
                        }
                        else
                        {
                            stream.WriteByte(0);
                        }

                    }
                    else
                    {
                        stream.WriteByte(world.Bytes[i]);
                    }

                }
            }
        }

        private World(byte[] bytes, Dictionary<int, Point> chunks, string name, Rectangle worldArea)
        {
            Bytes = bytes;
            Chunks = chunks;
            Name = name;
            WorldArea = worldArea;
        }
    }
}
