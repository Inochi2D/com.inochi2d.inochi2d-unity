using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Data;
using System.Buffers.Binary;

namespace Inochi2D.IO {

    public enum INPTextureFormat {
        PNG = 0,
        TGA = 1,
        BC7 = 2,
    }

    public struct INPTexture {

        /// <summary>
        /// Format of the INP texture
        /// </summary>
        public INPTextureFormat Format;

        /// <summary>
        /// Data of the INP texture
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Gets a Unity Texture2D from the INP Texture
        /// </summary>
        /// <returns>A Texture2D if the texture format is supported, otherwise returns null</returns>
        public Texture2D ToTexture2D(bool premultiply=true) {
            Texture2D tex = null;
            if (Format == INPTextureFormat.PNG) {
                tex = new Texture2D(2, 2);
                tex.LoadImage(Data);
            }

            if (Format == INPTextureFormat.TGA) {
                tex = TGALoader.LoadTGA(new MemoryStream(Data));
            }

            // TODO: properly specify BC7 format
            // For now we read 2 uints
            if (Format == INPTextureFormat.BC7) {
                uint width = BitConverter.ToUInt32(Data.AsSpan(0, 4));
                uint height = BitConverter.ToUInt32(Data.AsSpan(4, 4));
                tex = new Texture2D((int)width, (int)height, TextureFormat.BC7, false);
                tex.LoadRawTextureData(Data.AsSpan(8).ToArray());
            }

            tex.alphaIsTransparency = true;
            if (premultiply) this.Premultiply(ref tex);

            return tex;
        }

        private void Premultiply(ref Texture2D tex) {
            // Colors in Inochi2D should ALWAYS be premultiplied
            Color[] pixels = tex.GetPixels(0);
            for(int i = 0; i < pixels.Length; i++) {
                pixels[i].r *= pixels[i].a;
                pixels[i].g *= pixels[i].a;
                pixels[i].b *= pixels[i].a;
            }

            tex.SetPixels(pixels);
        }
    }

    struct INPReaderSettings {

    }

    /// <summary>
    /// INP Reader, takes a stream and reads the structure of the INP file
    /// Currently reads INP v1 files.
    /// </summary>
    public class INPReader : BinaryReader {
        private string mPayload;
        private INPTexture[] mTextures;

        static string INP_HEADER = "TRNSRTS\0";
        static string TEX_HEADER = "TEX_SECT";
        static string EXT_HEADER = "EXT_SECT";

        string ReadStringLen(int length) {
            return new string(this.ReadChars(length));
        }

        private bool VerifyMagicBytes(string bytes) {
            return this.ReadStringLen(bytes.Length) == bytes;
        }

        private void CheckEntyMagic() {
            string magicString = ReadStringLen(8);
            if (magicString != INP_HEADER) throw new Exception($"Invalid magic bytes, was \"{magicString}\"!");
        }

        private void ReadSections() {
            // Get payload position
            uint payloadLength = BinaryPrimitives.ReadUInt32BigEndian(this.ReadBytes(4));
            Debug.Log($"Payload Length = {payloadLength}");

            mPayload = new string(this.ReadChars((int)payloadLength));

            // Verifies the magic bytes for the texture header
            string magicString = ReadStringLen(8);
            if (magicString != TEX_HEADER) throw new Exception($"Expected Texture header, got \"{magicString}\"!");

            // Get texture count
            uint textureCount = BinaryPrimitives.ReadUInt32BigEndian(this.ReadBytes(4));

            // Read textures out
            List<INPTexture> textureBuffer = new List<INPTexture>();
            for(int index = 0; index < textureCount; index++) {

                // Get Texture payload length
                uint texturePayloadLength = BinaryPrimitives.ReadUInt32BigEndian(this.ReadBytes(4));

                // Get texture format
                INPTextureFormat textureFormat = (INPTextureFormat)this.ReadByte();

                textureBuffer.Add(new INPTexture { Data = this.ReadBytes((int)texturePayloadLength), Format = textureFormat });
            }
            mTextures = textureBuffer.ToArray();

            // TODO: EXT SECTION

        }

        /// <summary>
        /// Constructs an INP reader
        /// </summary>
        /// <param name="stream">file or memory stream of the puppet</param>
        public INPReader(Stream stream) : base(stream, encoding: Encoding.UTF8) {
            this.CheckEntyMagic();
        }

        /// <summary>
        /// Constructs an INP reader
        /// </summary>
        /// <param name="file">Path to INP file to load</param>
        public INPReader(string file) : this(new FileStream(file, FileMode.Open)) { }

        /// <summary>
        /// Constructs an INP reader
        /// </summary>
        /// <param name="data">Binary INP file data</param>
        public INPReader(byte[] data) : this(new MemoryStream(data)) { }

        public void ReadFile() {
            ReadSections();
        }

        /// <summary>
        /// Gets the raw model payload from the model file.
        /// 
        /// ReadFile has to be called first.
        /// </summary>
        /// <returns>JSON string of the payload</returns>
        public string PayloadJSON { get { return mPayload; } }

        /// <summary>
        /// Gets the raw model payload from the model file.
        /// 
        /// ReadFile has to be called first.
        /// </summary>
        /// <returns>JSON string of the payload</returns>
        public INPTexture[] Textures { get { return mTextures; } }

        /// <summary>
        /// Gets the model from the stream, setting up textures and everything.
        /// 
        /// ReadFile has to be called first.
        /// </summary>
        /// <returns></returns>
        public Puppet ReadPuppet() {
            return JsonUtility.FromJson<Puppet>(PayloadJSON);
        }

        /// <summary>
        /// Reads the extended vendor section from the INP file
        /// </summary>
        /// <returns></returns>
        public byte[][] ReadExtendedSection() {
            throw new NotImplementedException();
        }
    }
}