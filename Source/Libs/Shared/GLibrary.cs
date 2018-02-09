using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class LibraryDetails
{
    public Library Library { get; set; }
    public string WindowsLib { get; set; }
    public string LinuxLib { get; set; }
    public string OSXLib { get; set; }

    public LibraryDetails(Library lib, string windowsLib, string linuxLib, string osxLib)
    {
        this.Library = lib;
        this.WindowsLib = windowsLib;
        this.LinuxLib = linuxLib;
        this.OSXLib = osxLib;
    }
}

class GLibrary
{
    private static Dictionary<Library, IntPtr> _libraries;
    private static Dictionary<string, IntPtr> _customlibraries;
    private static List<LibraryDetails> _libdict;

    static GLibrary()
    {
        _customlibraries = new Dictionary<string, IntPtr>();
        _libraries = new Dictionary<Library, IntPtr>();
        _libdict = new List<LibraryDetails>();
        _libdict.Add(new LibraryDetails(Library.GLib, "libglib-2.0-0.dll", "libglib-2.0.so.0", "libglib-2.0.0.dylib"));
        _libdict.Add(new LibraryDetails(Library.GObject, "libgobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib"));
        _libdict.Add(new LibraryDetails(Library.Cairo, "libcairo-2.dll", "libcairo.so.2", "libcairo.2.dylib"));
        _libdict.Add(new LibraryDetails(Library.Gio, "libgio-2.0-0.dll", "libgio-2.0.so.0", "libgio-2.0.0.dylib"));
        _libdict.Add(new LibraryDetails(Library.Atk, "libatk-1.0-0.dll", "libatk-1.0.so.0", "libatk-1.0.0.dylib"));
        _libdict.Add(new LibraryDetails(Library.Pango, "libpango-1.0-0.dll", "libpango-1.0.so.0", "libpango-1.0.0.dylib"));
        _libdict.Add(new LibraryDetails(Library.Gdk, "libgdk-3-0.dll", "libgdk-3.so.0", "libgdk-3.0.dylib"));
        _libdict.Add(new LibraryDetails(Library.GdkPixbuf, "libgdk_pixbuf-2.0-0.dll", "libgdk_pixbuf-2.0.so.0", "libgdk_pixbuf-2.0.dylib"));
        _libdict.Add(new LibraryDetails(Library.Gtk, "libgtk-3-0.dll", "libgtk-3.so.0", "libgtk-3.0.dylib"));
        _libdict.Add(new LibraryDetails(Library.PangoCairo, "libpangocairo-1.0-0.dll", "libpangocairo-1.0.so.0", "libpangocairo-1.0.0.dylib"));
    }

    public static IntPtr Load(string libname)
    {
        var index = _libdict.FindIndex((e) => (e.WindowsLib == libname || e.LinuxLib == libname || e.OSXLib == libname));

        if (index != -1)
            return Load(_libdict[index].Library);

        var ret = IntPtr.Zero;
        if (!_customlibraries.TryGetValue(libname, out ret))
            _customlibraries[libname] = ret = FuncLoader.LoadLibrary(libname);

        return ret;
    }

    public static IntPtr Load(Library library)
    {
        IntPtr ret = IntPtr.Zero;
        if (!_libraries.TryGetValue(library, out ret))
        {
            var i = _libdict.Find((e) => e.Library == library);
            var s = i.LinuxLib;

            if (FuncLoader.IsWindows)
                s = i.WindowsLib;
            else if (FuncLoader.IsOSX)
                s = i.OSXLib;

            _libraries[library] = ret = FuncLoader.LoadLibrary(s);
        }

        if (ret == IntPtr.Zero)
            throw new DllNotFoundException(library.ToString());

        return ret;
    }
}