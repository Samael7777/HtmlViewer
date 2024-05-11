namespace HtmlViewer.Models;

public enum ContentLoadStatus
{
    NotLoaded,
    Loading,
    Loaded,
    NotFound,
    LoadError,
    Saving,
    SaveError,
    Saved
}