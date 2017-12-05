// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IXmlSerializerEx.cs" company="">
//   
// </copyright>
// <summary>
//   The XmlSerializerEx interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Interfaces
{
    using System.IO;
    using System.Xml;

    /// <summary>
    /// The XmlSerializerEx interface.
    /// </summary>
    public interface IXmlSerializerEx
    {
        #region Public Methods and Operators

        /// <summary>
        /// The deserialize.
        /// </summary>
        /// <param name="s">
        /// The s.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        object Deserialize(Stream s);

        /// <summary>
        /// The serialize.
        /// </summary>
        /// <param name="xmlWriter">
        /// The xml writer.
        /// </param>
        /// <param name="o">
        /// The o.
        /// </param>
        void Serialize(XmlWriter xmlWriter, object o);

        #endregion
    }
}