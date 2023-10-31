using System.Collections.Generic;

namespace GameModel {
    public static class Suit {
        public static string MAN = "m"; // characters
        public static string SOU = "s"; // bamboo
        public static string PIN = "p"; // dots
        public static string HONOR = "z";

        public static List<string> NOT_HONORS = new List<string> { MAN, SOU, PIN };
    }
}