using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WF.PdfLib.Services.Bookmarks;

namespace WF.PdfLib.Common
{
    public class Bookmark : IBookmark
    {
        public Guid Id { get; }

        public virtual string Title { get; set; }

        public IList<int> Pages { get; }

        public int StartPage { get => Pages[0]; }

        public int EndPage { get => Pages.Last(); }

        public IList<IBookmark> Children { get; }

        public virtual int Level { get; set; }

        public Guid ParentId { get; }

        public string FilePath { get; }

        public string FileName { get; }

        public string FileNameExtension { get; }

        public string FileNameWithExtension { get; }

        public virtual bool IsSelected { get; set; }

        public virtual int Index { get; set; }

        public bool IsFileless { get; set; }

        public string FormattedRange
        {
            get => Pages.Count < 1 ? "-" : StartPage + "-" + EndPage;
        }

        public Bookmark
        (
            string title,
            IList<int> pages,
            int level,
            IList<IBookmark> children,
            Guid id,
            Guid parentId,
            string filePath,
            int index)
        {
            Title = title;
            Pages = pages;
            Level = level;
            Children = children;
            Id = id;
            ParentId = parentId;
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            FileNameExtension = Path.GetExtension(filePath).ToLower();
            FileNameWithExtension = Path.GetFileName(filePath);
            Index = index;
        }
    }
}
