module App 
open System
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Media.Imaging
open System.Windows.Threading
open System.Linq
open System.Collections.Generic
open FsXaml

type MainWindowBase = XAML<"MainWindow.xaml">

type MainWindow() as this = 
    inherit MainWindowBase()

    let gameTimer = new DispatcherTimer()
    let rand = new Random()
    let mutable playerHitBox : Rect = new Rect(Canvas.GetLeft(this.playerShip),Canvas.GetTop(this.playerShip), this.playerShip.Width, this.playerShip.Height)
    let mutable moveLeft: bool = false
    let mutable moveRight: bool = false
    let mutable itemRemover = new List<Rectangle>()
    let mutable enemyShipCounter: int = 0
    let mutable enemyCount: int = 100
    let mutable playerSpeed: int = 10
    let mutable limit: int = 60
    let mutable score: int = 0
    let mutable damage: int = 0
    let mutable enemySpeed: int = 10

    let btnClick _ =
        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location) |> ignore
        Application.Current.Shutdown()
    

    let KeyDown_fun (e:KeyEventArgs) = 
        if e.Key = Key.Left then
            moveLeft <- true

        if e.Key = Key.Right then
            moveRight <- true

    let KeyUp_fun (e:KeyEventArgs) = 
        
        if e.Key = Key.Left then
            moveLeft <- false

        if e.Key = Key.Right then
            moveRight <- false
        
        if e.Key = Key.Space then
            let laserImage = new ImageBrush()     
            laserImage.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/player_laser.png"))
            let newlaser : Rectangle = new Rectangle(Tag ="laser", Height = 20.0, Width = 10.0, Fill = laserImage, Stroke = null)
            Canvas.SetLeft(newlaser, Canvas.GetLeft(this.playerShip) + this.playerShip.Width/2.0)
            Canvas.SetTop(newlaser, Canvas.GetTop(this.playerShip) - newlaser.Height)
            this.MyCanvas.Children.Add(newlaser) |> ignore
    

    let SetPlayerHitBox _ = 
        playerHitBox.X <- Canvas.GetLeft(this.playerShip)
        playerHitBox.Y <- Canvas.GetTop(this.playerShip)
        playerHitBox.Width <- this.playerShip.Width
        playerHitBox.Height <- this.playerShip.Height
        
    let RemoveSprites _ = 
        for y in Enumerable.OfType<Rectangle>(itemRemover) do
            this.MyCanvas.Children.Remove(y)
            
    let GenerateEnemyShips _ = 
        

        let enemySprite = new ImageBrush()
        enemyShipCounter <- rand.Next(1,7)

        match enemyShipCounter with
        |1 -> enemySprite.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/1.png"))
        |2 -> enemySprite.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/2.png"))
        |3 -> enemySprite.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/3.png"))    
        |4 -> enemySprite.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/4.png"))
        |5 -> enemySprite.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/5.png"))
        |6 -> enemySprite.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/6.png"))
        |7 -> enemySprite.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/7.png"))
        |_ -> ()

        let newEnemy = new Rectangle(Tag ="enemy", Height = 50.0, Width = 56.0, Fill = enemySprite)
        Canvas.SetTop(newEnemy, -100.0)
        Canvas.SetLeft(newEnemy, float(rand.Next(30,430)))
        this.MyCanvas.Children.Add(newEnemy) |> ignore

    let GameLoop(e:EventArgs) = 
        
        SetPlayerHitBox null
        enemyCount <- enemyCount - 1
        this.scoreText.Content <- "Score: " + score.ToString()
        
        if enemyCount < 0 then
            GenerateEnemyShips null
            enemyCount <- limit
        
        if moveLeft = true && Canvas.GetLeft(this.playerShip) > 0.0 then
            Canvas.SetLeft(this.playerShip,Canvas.GetLeft(this.playerShip) - float(playerSpeed))
        
        if moveRight = true && Canvas.GetLeft(this.playerShip) + 80.0 < Application.Current.MainWindow.Width then
            Canvas.SetLeft(this.playerShip,Canvas.GetLeft(this.playerShip) + float(playerSpeed))
        
        for x in Enumerable.OfType<Rectangle>(this.MyCanvas.Children) do
            if x.Tag <> null then 
                if x.Tag.ToString() = "laser" then
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20.0)
                    let laserHitBox = new Rect(Canvas.GetLeft(x),Canvas.GetTop(x),x.Width,x.Height)
                    if Canvas.GetTop(x) < 10.0 then
                        itemRemover.Add(x)
                    for y in Enumerable.OfType<Rectangle>(this.MyCanvas.Children) do
                        if y.Tag <> null then
                            if y.Tag.ToString() = "enemy" then
                                let enemyHit = new Rect(Canvas.GetLeft(y),Canvas.GetTop(y),y.Width,y.Height)
                                if laserHitBox.IntersectsWith(enemyHit) then
                                    itemRemover.Add(x)
                                    itemRemover.Add(y)
                                    score <- score + 1

                if x.Tag.ToString() = "enemy" then
                    Canvas.SetTop(x, Canvas.GetTop(x) + float(enemySpeed))
                    if Canvas.GetTop(x) > float(750) then
                        itemRemover.Add(x)
                        damage <- damage + 8
                        this.healthBar.Value <- 100.0 - float(damage)
                    let enemyHitBox = new Rect(Canvas.GetLeft(x),Canvas.GetTop(x),x.Width,x.Height)
                    if playerHitBox.IntersectsWith(enemyHitBox) then
                        itemRemover.Add(x)
                        damage <- damage + 5
                        this.healthBar.Value <- 100.0 - float(damage)
        
        RemoveSprites null
        
        if damage > 70 then
            this.healthBar.Foreground <- Brushes.Red
        if damage > 40 && damage < 70 then
            this.healthBar.Foreground <- Brushes.Yellow

        if score > 8 then
            limit <- 25
            enemySpeed <- 13
        
        if damage > 99 then
            gameTimer.Stop()

            for y in Enumerable.OfType<Rectangle>(this.MyCanvas.Children) do
                if y.Tag <> null then
                    if y.Tag.ToString() = "enemy" || y.Tag.ToString() = "laser" then
                        itemRemover.Add(y)

            RemoveSprites null

            this.gameOver.Visibility <- Visibility.Visible

            this.gameOverContent.Content <- "You have destroyed " + score.ToString() + " Enemies " + "Press OK to play again. Space bar is for bullets"
                
    do

        this.KeyUp.Add KeyUp_fun
        this.KeyDown.Add KeyDown_fun
        this.OkButton.Click.Add btnClick
        this.gameOver.Visibility <- Visibility.Hidden
        
        let PlayerImg = new ImageBrush()     
        PlayerImg.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/player.png"))
        this.playerShip.Fill <- PlayerImg
        this.MyCanvas.Focus() |> ignore

        let bg = new ImageBrush()
        bg.ImageSource <- new BitmapImage(new Uri("pack://application:,,,/images/background.png"))
        bg.TileMode <- TileMode.Tile
        bg.Viewport <- new Rect(0.0,0.0,0.15,0.15)
        bg.ViewportUnits <- BrushMappingMode.RelativeToBoundingBox
        this.MyCanvas.Background <- bg
        this.Title <- "SpaceShip Battle"

        gameTimer.Interval <- TimeSpan.FromMilliseconds(20.0)
        gameTimer.Tick.Add GameLoop
        gameTimer.Start()
      

[<STAThread>]
[<EntryPoint>]
let application = new Application() in
    let mainWindow = new MainWindow() in
        application.Run(mainWindow) |> ignore    
    


