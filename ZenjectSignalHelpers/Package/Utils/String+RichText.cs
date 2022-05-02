namespace ZenjectSignalHelpers.Utils
{
    internal static class StringRichText
    {
        /// <summary>
        /// Add bold tags to a string.
        /// </summary>
        public static string Bold(this string s) => $"<b>{s}</b>";

        /// <summary>
        /// Add color tags to a string.
        /// </summary>
        public static string Color(this string s, string color) => color != null ? $"<color={color}>{s}</color>" : s;

        /// <summary>
        /// Add italic tags to a string.
        /// </summary>
        public static string Italic(this string s) => $"<i>{s}</i>";

        /// <summary>
        /// Add size tag to a string.
        /// </summary>
        public static string Size(this string s, int size) => $"<size={size.ToString()}>{s}</size>";
    }
}