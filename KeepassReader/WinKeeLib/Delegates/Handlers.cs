// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Handlers.cs" company="">
//   
// </copyright>
// <summary>
//   Function definition of a method that performs an action on a group.
//   When traversing the internal tree, this function will be invoked
//   for all visited groups.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Delegates
{
    /// <summary>
    /// Function definition of a method that performs an action on a group.
    /// When traversing the internal tree, this function will be invoked
    /// for all visited groups.
    /// </summary>
    /// <param name="pg">Currently visited group.</param>
    /// <returns>You must return <c>true</c> if you want to continue the
    /// traversal. If you want to immediately stop the whole traversal,
    /// return <c>false</c>.</returns>
    public delegate bool GroupHandler(PwGroup pg);

    /// <summary>
    /// Function definition of a method that performs an action on an entry.
    /// When traversing the internal tree, this function will be invoked
    /// for all visited entries.
    /// </summary>
    /// <param name="pe">Currently visited entry.</param>
    /// <returns>You must return <c>true</c> if you want to continue the
    /// traversal. If you want to immediately stop the whole traversal,
    /// return <c>false</c>.</returns>
    public delegate bool EntryHandler(PwEntry pe);

    /// <summary>
    /// The void delegate.
    /// </summary>
    public delegate void VoidDelegate();

    /// <summary>
    /// The str pw entry delegate.
    /// </summary>
    /// <param name="str">
    /// The str.
    /// </param>
    /// <param name="pe">
    /// The pe.
    /// </param>
    public delegate string StrPwEntryDelegate(string str, PwEntry pe);
}