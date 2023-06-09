﻿namespace ProyectoVideoteca.Models
{
    public class tb_MOVIEANDCOMMENTS
    {

        public tb_MOVIE movie { get; set; }

        public List<tb_RATING> comments { get; set; }

        public int itemsPerPage { get; set; }

        public int totalPages { get; set; }

        public int currentPage { get; set; }

        public tb_MOVIEANDCOMMENTS(tb_MOVIE movie, List<tb_RATING> comments, int itemsPerPage, int totalPages, int currentPage)
        {
            this.movie = movie;
            this.comments = comments;
            this.itemsPerPage = itemsPerPage;
            this.totalPages = totalPages;
            this.currentPage = currentPage;
        }
    }
}
