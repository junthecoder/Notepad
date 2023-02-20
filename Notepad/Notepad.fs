module Notepad

open System
open System.IO
open System.Text.RegularExpressions
open System.Windows.Forms

[<STAThread>]
[<EntryPoint>]
let main args =

    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault false

    use form = new Form(Width = 800, Height = 600, Text = "Notepad")

    let textBox = new TextBox(Multiline = true,
                              MaxLength = Int32.MaxValue,
                              ScrollBars = ScrollBars.Both,
                              Dock = DockStyle.Fill)
    let statusStrip = new StatusStrip(Dock = DockStyle.Bottom)
    let statusLabel = new ToolStripStatusLabel()
    statusStrip.Items.Add(statusLabel) |> ignore

    let updateStatusLabel(_) =
        let headToSelectionStart = textBox.Text.Substring(0, textBox.SelectionStart)
        let line = Regex.Matches(headToSelectionStart, "\n").Count + 1
        let col = textBox.SelectionStart - headToSelectionStart.LastIndexOf("\n")
        statusLabel.Text <- sprintf "Ln %d, Col %d" line col

    textBox.KeyUp.Add(updateStatusLabel)
    textBox.MouseClick.Add(updateStatusLabel)
    textBox.TextChanged.Add(updateStatusLabel)

    let menuStrip = new MenuStrip(Dock = DockStyle.Top)
    let addMenu text =
        let menu = new ToolStripMenuItem(Text = text)
        menuStrip.Items.Add(menu) |> ignore
        menu

    let fileMenu = addMenu "&File"
    let editMenu = addMenu "&Edit"
    let formatMenu = addMenu "F&ormat"
    let viewMenu = addMenu "&View"
    let helpMenu = addMenu "&Help"

    let addMenuItem (menu : ToolStripMenuItem) text key onClick =
        let item = new ToolStripMenuItem(Text = text, ShortcutKeyDisplayString = key, AutoSize = true)
        menu.DropDownItems.Add(item) |> ignore
        item.Click.Add(onClick)
        item

    let addSeparator (menu : ToolStripMenuItem) =
        let separator = new ToolStripSeparator()
        menu.DropDownItems.Add(separator) |> ignore
        separator

    let save() =
        let dialog = new SaveFileDialog()
        let result = dialog.ShowDialog()
        result

    let addToFileMenu = addMenuItem fileMenu
    let addSeparatorToFileMenu() = addSeparator fileMenu

    let newMenuItem = addToFileMenu "&New" "Ctrl+N" (fun _ -> ())
    let openMenuItem = addToFileMenu "&Open..." "Ctrl+O" (fun _ ->
        let dialog = new OpenFileDialog()
        match dialog.ShowDialog() with
        | DialogResult.OK ->
            use reader = new StreamReader(dialog.FileName)
            textBox.Text <- reader.ReadToEnd()
            textBox.Modified <- false
        | _ -> ()
    )
    let saveMenuItem = addToFileMenu "&Save" "Ctrl+S" (fun _ -> ())
    let saveAsMenuItem = addToFileMenu "Save &As..." "" (fun _ -> save() |> ignore)
    let separator = addSeparatorToFileMenu()
    let exitMenuItem = addToFileMenu "E&xit" "" (fun _ ->
        match textBox.Modified with
        | true ->
            let result = MessageBox.Show("Do you want to save changes?", "Notepad", MessageBoxButtons.YesNoCancel)
            match result with
            | DialogResult.Yes ->
                match save() with
                | DialogResult.OK -> Application.Exit()
                | _ -> ()
            | DialogResult.No -> Application.Exit()
            | _ -> ()
        | false ->
            Application.Exit()
    )

    let _ = addMenuItem editMenu "&Undo" "Ctrl+Z" (fun _ -> textBox.Undo())
    let _ = addSeparator editMenu
    let _ = addMenuItem editMenu "Cu&t" "Ctrl+X" (fun _ -> textBox.Cut())
    let _ = addMenuItem editMenu "&Copy" "Ctrl+C" (fun _ -> textBox.Copy())
    let _ = addMenuItem editMenu "&Paste" "Ctrl+V" (fun _ -> textBox.Paste())
    let _ = addMenuItem editMenu "De&lete" "Del" (fun _ -> textBox.SelectedText <- "")
    let _ = addSeparator editMenu
    let _ = addMenuItem editMenu "&Find..." "Ctrl+F" (fun _ -> ())
    let _ = addMenuItem editMenu "Find &Next" "F3" (fun _ -> ())
    let _ = addMenuItem editMenu "&Replace..." "Ctrl+H" (fun _ -> ())
    let _ = addMenuItem editMenu "&Go To..." "Ctrl+G" (fun _ -> ())
    let _ = addSeparator editMenu
    let _ = addMenuItem editMenu "Select &All" "Ctrl+A" (fun _ -> textBox.SelectAll())

    let _ = addMenuItem formatMenu "&Font..." "" (fun _ ->
        let dialog = new FontDialog(Font = textBox.Font)
        let result = dialog.ShowDialog()
        match result with
        | DialogResult.OK -> textBox.Font <- dialog.Font
        | _ -> ()
    )
    let _ = addMenuItem viewMenu "&Status Bar" "" (fun _ -> (statusStrip.Visible <- not statusStrip.Visible))
    let _ = addMenuItem helpMenu "&About..." "" (fun _ ->
        MessageBox.Show(Application.ProductName + " " + Application.ProductVersion, "About") |> ignore)

    form.Controls.Add(textBox)
    form.Controls.Add(menuStrip)
    form.Controls.Add(statusStrip)

    Application.Run(form)

    0
