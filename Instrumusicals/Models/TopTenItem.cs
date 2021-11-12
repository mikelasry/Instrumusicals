using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class TopTenItem
    {
        public TopTenItem(string albumName, string artistName, string yearReleased, string style, string case3D, string sales)
        {
            AlbumName = albumName;
            ArtistName = artistName;
            YearReleased = yearReleased;
            Style = style;
            Case3D = case3D;
            Sales = sales;
        }

        public string AlbumName { get; set; }
        public string ArtistName { get; set; }

        public string YearReleased { get; set; }

        public string Style { get; set; }

        public string Case3D { get; set; }

        public string Sales { get; set; }


    }
}
