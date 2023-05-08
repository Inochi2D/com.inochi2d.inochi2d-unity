// Based on TGA loader made by aaro4130 on the Unity forums.
// Loader has been optimized and RLE support has been added

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Inochi2D.IO {
    public static class TGALoader {

        /// <summary>
        /// Loads a TGA to a Texture2D from memory
        /// </summary>
        /// <param name="TGAStream"></param>
        /// <returns></returns>
        public static Texture2D LoadTGA(Stream TGAStream) {
            using (BinaryReader r = new BinaryReader(TGAStream)) {

                // Skip to encoding info
                r.BaseStream.Seek(2, SeekOrigin.Begin);
                byte encoding = r.ReadByte();

                // Skip some header info we don't care about.
                // Even if we did care, we have to move the stream seek point to the beginning,
                // as the previous method in the workflow left it at the end.
                r.BaseStream.Seek(12, SeekOrigin.Begin);

                short width = r.ReadInt16();
                short height = r.ReadInt16();
                int bitDepth = r.ReadByte();
                int channels = bitDepth / 8;

                // Skip a byte of header information we don't care about.
                r.BaseStream.Seek(1, SeekOrigin.Current);

                Texture2D tex = new Texture2D(width, height);
                Color32[] pulledColors = new Color32[width * height];
                switch(encoding) {
                    case 2: // Uncompressed
                        if (bitDepth == 32) {
                            for (int i = 0; i < width * height; i++) {
                                byte red = r.ReadByte();
                                byte green = r.ReadByte();
                                byte blue = r.ReadByte();
                                byte alpha = r.ReadByte();

                                pulledColors[i] = new Color32(blue, green, red, alpha);
                            }
                        } else if (bitDepth == 24) {
                            for (int i = 0; i < width * height; i++) {
                                byte red = r.ReadByte();
                                byte green = r.ReadByte();
                                byte blue = r.ReadByte();

                                pulledColors[i] = new Color32(blue, green, red, 255);
                            }
                        } else {
                            throw new Exception("TGA texture had non 32/24 bit depth.");
                        }
                        break;
                    case 10: // RLE Encoded
                        if (!(channels == 3 || channels == 4)) throw new Exception("TGA texture had non 32/24 bit depth.");


                        int slinez = width * channels;
                        int packetLen = 0;
                        bool isRLE = false;

                        for (int y = 0; y < height; y++) {
                            
                            // Scanline begin
                            int sline = width * y;
                            int wanted = slinez;
                            do {
                                if (packetLen == 0) {

                                    // Packet header
                                    byte packetHead = r.ReadByte();
                                    isRLE = (packetHead & 0x80) != 0;
                                    packetLen = ((packetHead & 0x7f) + 1) * channels;
                                }

                                int gotten = slinez - wanted;
                                int copySize = wanted < packetLen ? wanted : packetLen;


                                // NOTE: Textures in RLE mode are encoded as BGRA, color fetching is flipped to account for it.
                                if (isRLE) {

                                    // Row Length Encoded
                                    byte[] colors = r.ReadBytes(channels);
                                    for (int p = gotten; p < gotten + copySize; p += channels) {
                                        pulledColors[sline + (p / channels)] = new Color32(colors[2], colors[1], colors[0], channels == 4 ? colors[3] : (byte)255); 
                                    }
                                } else {

                                    // Raw Encoded
                                    for (int p = gotten; p < gotten+copySize; p += channels) {
                                        byte[] colors = r.ReadBytes(channels);
                                        pulledColors[sline + (p / channels)] = new Color32(colors[2], colors[1], colors[0], channels == 4 ? colors[3] : (byte)255);
                                    }
                                }
                                wanted -= copySize;
                                packetLen -= copySize;
                            } while (wanted > 0);
                        }
                        break;
                    default:
                        throw new Exception($"TGA encoding format {encoding} not supported!");
                }

                tex.SetPixels32(pulledColors);
                tex.Apply();
                return tex;

            }
        }
    }
}