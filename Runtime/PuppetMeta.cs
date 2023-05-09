using Newtonsoft.Json;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;

namespace Inochi2D {

    [System.Serializable]
    public enum PuppetAllowedUsers {
        /// <summary>
        /// Only the author(s) are allowed to use the puppet
        /// </summary>
        [EnumMember(Value = "onlyAuthor")]
        OnlyAuthor,

        /// <summary>
        /// Only licensee(s) are allowed to use the puppet
        /// </summary>
        [EnumMember(Value = "onlyLicensee")]
        OnlyLicensee,

        /// <summary>
        /// Everyone may use the model
        /// </summary>
        [EnumMember(Value = "everyone")]
        Everyone
    }

    [System.Serializable]
    public enum PuppetAllowedRedistribution {
        /// <summary>
        /// Redistribution is prohibited
        /// </summary>
        [EnumMember(Value = "prohibited")]
        Prohibited,

        /// <summary>
        /// Redistribution is allowed, but only under the same license as the original.
        /// </summary>
        [EnumMember(Value = "viralLicense")]
        ViralLicense,

        /// <summary>
        /// Redistribution is allowed, and the puppet may be redistributed under a different
        /// license than the original.
        /// 
        /// This goes in conjunction with modification rights.
        /// </summary>
        [EnumMember(Value = "copyleftLicense")]
        CopyleftLicense
    }

    [System.Serializable]
    public enum PuppetAllowedModification {
        /// <summary>
        /// Modification is prohibited
        /// </summary>
        [EnumMember(Value = "prohibited")]
        Prohibited,

        /// <summary>
        /// Modification is only allowed for personal use.
        /// </summary>
        [EnumMember(Value = "allowPersonal")]
        AllowPersonal,

        /// <summary>
        /// Modification is allowed with redistribution, see allowedRedistribution for redistribution terms.
        /// </summary>
        [EnumMember(Value = "allowRedistribute")]
        AllowRedistribute
    }

    [System.Serializable]
    public class PuppetUsageRights {
        /// <summary>
        /// Who is allowed to use the puppet?
        /// </summary>
        [JsonProperty("allowedUsers")]
        [OptionalField]
        public PuppetAllowedUsers AllowedUsers = PuppetAllowedUsers.OnlyAuthor;

        /// <summary>
        /// Whether violence content is allowed
        /// </summary>
        [JsonProperty("allowViolence")]
        [OptionalField]
        public bool AllowViolence = false;

        /// <summary>
        /// Whether sexual content is allowed
        /// </summary>
        [JsonProperty("allowSexual")]
        [OptionalField]
        public bool AllowSexual = false;

        /// <summary>
        /// Whether commerical use is allowed
        /// </summary>
        [JsonProperty("allowCommercial")]
        [OptionalField]
        public bool AllowCommercial = false;

        /// <summary>
        /// Whether a model may be redistributed
        /// </summary>
        [JsonProperty("allowRedistribution")]
        [OptionalField]
        public PuppetAllowedRedistribution AllowRedistribution = PuppetAllowedRedistribution.Prohibited;

        /// <summary>
        /// Whether a model may be modified
        /// </summary>
        [JsonProperty("allowModification")]
        [OptionalField]
        public PuppetAllowedModification AllowModification = PuppetAllowedModification.Prohibited;

        /// <summary>
        /// Whether the author(s) must be attributed for use.
        /// </summary>
        [JsonProperty("requireAttribution")]
        [OptionalField]
        public bool RequireAttribution = false;
    }

    [System.Serializable]
    public class PuppetMeta {

        public const uint NoThumbnail = uint.MaxValue;

        /**
            Name of the puppet
        */
        [JsonProperty("name")]
        [OptionalField]
        public string Name;

        /**
            Version of the Inochi2D spec that was used for creating this model
        */
        [JsonProperty("version")]
        public string Version = "1.0-alpha";

        /**
            Rigger(s) of the puppet
        */
        [JsonProperty("rigger")]
        [OptionalField]
        public string Rigger;

        /**
            Artist(s) of the puppet
        */
        [JsonProperty("artist")]
        [OptionalField]
        public string Artist;

        /**
            Usage Rights of the puppet
        */
        [JsonProperty("rights")]
        [OptionalField]
        public PuppetUsageRights Rights;

        /**
            Copyright string
        */
        [JsonProperty("copyright")]
        [OptionalField]
        public string Copyright;

        /**
            URL of license
        */
        [JsonProperty("licenseURL")]
        [OptionalField]
        public string LicenseURL;

        /**
            Contact information of the first author
        */
        [JsonProperty("contact")]
        [OptionalField]
        public string Contact;

        /**
            Link to the origin of this puppet
        */
        [JsonProperty("reference")]
        [OptionalField]
        public string Reference;

        /**
            Texture ID of this puppet's thumbnail
        */
        [JsonProperty("thumbnailId")]
        [OptionalField]
        public uint ThumbnailId = NoThumbnail;

        /**
            Whether the puppet should preserve pixel borders.
            This feature is mainly useful for puppets which use pixel art.
        */
        [JsonProperty("preservePixels")]
        [OptionalField]
        public bool PreservePixels = false;
    }

    [System.Serializable]
    public class PuppetMetaContainer {
        [JsonProperty("meta")]
        public PuppetMeta Meta;
    }
}