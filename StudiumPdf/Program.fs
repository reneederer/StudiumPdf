open System.Diagnostics
open System.IO
open PdfSharp
open PdfSharp.Pdf
open PdfSharp.Drawing
open System.Drawing
open System.Text.RegularExpressions
open System

type Git =
    { Url : string
      LocalDir : string
    }

module Git =
    let git command dir =
        use p = new Process()
        p.StartInfo.WorkingDirectory <- dir
        p.StartInfo.FileName <- "git"
        p.StartInfo.Arguments <- command
        p.Start() |> ignore
        p.WaitForExit()

    let pull = git "pull"
    let addAll = git "add ."
    let commit msg = git $"commit -m {msg}"
    let push = git "push origin master"


[<EntryPoint>]
let main args =
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
    let localDir = "c:/users/rene/Studium"
    //let repo = "https://github.com/reneederer/Studium"
    use watcher = new FileSystemWatcher(localDir)
    //if not <| Direc.net user directorytory.Exists (Path.Combine(localDir, ".git")) then
    //    Git.git localDir "init"
    //    Git.git localDir "remote add origin https://github.com/reneederer/Studium"
    //    Git.git localDir """config --global credential.helper "cache --timeout=157680000" """
    //    Git.git localDir """config --global credential.helper "cache --timeout=157680000" """
    //    Git.git localDir """git config --global user.email "rene.ederer@protonmail.com" """
    //    Git.git localDir """git config --global user.name "reneederer" """
    //Git.pull localDir

    //use doc = new Spire.Pdf.PdfDocument @"C:\Users\rene\Desktop\AddImage.pdf"
    let f = fun args ->
        let document = new PdfDocument()
        document.Options.CompressContentStreams <- true
        for dir in Directory.GetDirectories localDir |> Seq.filter (fun x -> Path.GetFileName x <> ".git") do
            let courseName = Path.GetFileName dir
            let mutable courseOutline = Unchecked.defaultof<PdfOutline>
            try
                for i, imagePath in Directory.GetFiles dir |> Seq.indexed do
                    try
                        let lectureDate, lectureName =
                            let m =
                                Regex.Match(
                                    Path.GetFileNameWithoutExtension imagePath,
                                    @"^v(\d{4}-\d{2}-\d{2}) (.*)$")
                            if not <| m.Success then
                                failwith $"Falsches Dateiformat: {imagePath}"
                            m.Groups.[1].Value, m.Groups.[2].Value
                        let image = XImage.FromFile imagePath
                        let page = document.AddPage()
                        if i = 0 then
                            courseOutline <- document.Outlines.Add(courseName, page, true, PdfOutlineStyle.Bold)
                        courseOutline.Outlines.Add($"{lectureName} ({lectureDate})", page, true) |> ignore
                        let gfx = XGraphics.FromPdfPage(page);
                        gfx.DrawImage(image, 0, 0)
                    with
                    | _ ->
                        ()
            with
            | _ ->
                ()
             
        document.Save(Path.Combine(localDir, "Studium.pdf"))
        Git.pull localDir
        Git.addAll localDir
        Git.commit "-" localDir
        Git.push localDir
    watcher.IncludeSubdirectories <- true;
    watcher.EnableRaisingEvents <- true;
    watcher.NotifyFilter <-
        NotifyFilters.Attributes ||| NotifyFilters.CreationTime |||
        NotifyFilters.DirectoryName ||| NotifyFilters.FileName |||
        NotifyFilters.LastAccess ||| NotifyFilters.LastWrite |||
        NotifyFilters.Security ||| NotifyFilters.Size
    watcher.Changed.Add f
    watcher.Deleted.Add f
    watcher.Created.Add f
    watcher.Renamed.Add f


    //Git.addAll localDir
    //Git.commit localDir "-"
    //Git.push localDir
    Console.ReadKey() |> ignore
    0







