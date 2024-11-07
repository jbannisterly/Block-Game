Option Strict On
Imports System.Console
Imports System.Runtime.InteropServices

Module Module1
    Public Const RENDERDISTANCE As Integer = 8
    Const LENGTHOFDAY As Single = 500
    Const TESTMODE As Boolean = False
    Const STARTTIME As Single = 0.3
    Const RELOADEXTERNALDATA As Boolean = False
    Public Const SMOOTHLIGHTINGENABLE As Boolean = True

    Sub Main(args As String())
        Dim openGlData As New OpenGL.OpenGlData(True)
        Dim soundData As New Sound.SoundData(True)

        If args.Length > 0 Then
            Sound.PlaySound(args(0), True)
            End
        End If

        Dim loadGame As Boolean
        Dim saveGame As String = ""
        If RELOADEXTERNALDATA Then
            If IO.Directory.Exists("Resource") Then IO.File.WriteAllText("ExternalData.txt", EmbedData.DataToString())
        End If
        If IO.File.Exists("ExternalData.txt") Then
            EmbedData.OutputData(IO.File.ReadAllText("ExternalData.txt"))
            If RELOADEXTERNALDATA Then
                End
            End If
        Else
            WriteLine("External Data not found")
        End If
        Dim blockData As ImportedData.BlockData() = ImportedData.ImportBlockData("Blocks")
        Dim itemData As ImportedData.ItemData() = ImportedData.ImportItemData("Items")
        Dim fuelData As ImportedData.FuelData() = ImportedData.ImportFuelData("Fuel")
        Dim smeltData As ImportedData.SmeltData() = ImportedData.ImportSmeltData("Smelt")
        Window.Initialise()
        MainMenu.Instructions()
        Window.Initialise()
        InitialiseBlockTextures(blockData, itemData, openGlData)
        MainMenu.MainMenu(loadGame, saveGame)
        MainGame(blockData, itemData, fuelData, smeltData, loadGame, saveGame, openGlData, soundData)
    End Sub

    Private Sub Endgame(ByRef allchunkChanges As FEN.ChunkChanges(), seed As Integer, saveGameName As String, ByRef furnaces As Furnace(), ByRef soundData As Sound.SoundData)
        MouseInput.ShowMouseCursor()
        For i = 0 To soundData.counter - 1
            Sound.CloseSound(i)
        Next
        IO.Directory.Delete("Resource", True)
        For i = 0 To RenderWorld.LoadedChunks.Length - 1
            If RenderWorld.LoadedChunks(i).inUse Then
                RenderWorld.BackupTorches(RenderWorld.LoadedChunks(i).artificialLightLocations, RenderWorld.LoadedChunks(i).artificialLightOrientations, New RenderWorld.ChunkCoord(RenderWorld.LoadedChunks(i).x, RenderWorld.LoadedChunks(i).z))
            End If
        Next
        FEN.OutputData(allchunkChanges, seed, RenderWorld.AllTorchData, saveGameName, furnaces)
        End
    End Sub

    Sub InitialiseBlockTextures(blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), ByRef openGlData As OpenGL.OpenGlData)
        Dim numTextures As Integer = 0
        For i = 0 To blockData.Length - 1
            numTextures += blockData(i).GenerateIndex(numTextures)
        Next
        RenderWorld.LoadTextures(blockData, numTextures, itemData, itemData.Length, openGlData)
        WriteLine(RenderWorld.NumTexturesTotal)
    End Sub

    'Public timer1, timer2, timer3, timer4 As Single
    'TEMP DELETE THESE

    Sub MainGame(blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), fuelData As ImportedData.FuelData(), smeltData As ImportedData.SmeltData(), loadGame As Boolean, saveGameName As String, ByRef openglData As OpenGL.OpenGlData, ByRef soundData As Sound.SoundData)
        Dim coords(100000000) As Single
        Dim result As IntPtr = Marshal.AllocHGlobal(10000000)
        Dim t As Single = CSng(Timer)
        Dim time As Double = Timer
        Dim deltaTime As Single
        Dim oldTime As Single
        Dim eToggle As Boolean
        Dim worldView As Boolean = True
        Dim inventorySelected As New Inventory.InventorySelected
        Dim oldMouse As MouseInput.MouseInput = MouseInput.GetInput()
        Dim key As New KeyboardInput.Keys
        Dim inventoryTextures As New List(Of Byte)
        Dim inventoryColours As New List(Of Byte)
        Dim inventoryCoords As New List(Of Single)
        Dim inventoryFraction As New List(Of OpenGL.FractionalIcon)
        Dim placeBlockToggle As Boolean = oldMouse.rightToggle
        Dim attackToggle As Boolean = oldMouse.leftToggle
        Dim currentInterface As Interfaces = Interfaces.Inventory
        Dim daytime As Single
        Dim c As Integer = 0
        Dim fps As Integer
        Dim elapsedTime As Integer
        Dim totalTime As Single
        Dim allChanges As FEN.ChunkChanges() = {}
        Dim lastFurnaceUpdate As Single
        Dim allFurnaces(10) As Furnace
        Dim furnaceIndex As Integer
        Dim seed As Integer
        Dim loadTime As Double = Timer
        Dim zombies(20) As Zombie
        Dim Randoms(100000) As Single
        Dim lastHeal As Integer = 0

        If loadGame Then
            FEN.InputData(saveGameName, allChanges, seed, RenderWorld.AllTorchData, allFurnaces)
        Else
            NewGame(seed)
        End If

        Random.Initialise(seed, Randoms)

        MouseInput.Initialise()
        MouseInput.ForceMouseMove(20, 20)
        ' MouseInput.ShowMouseCursor()

        RenderWorld.Initialise(blockData, allChanges, Randoms, openglData)
        If Not loadGame Then
            Player.Initialise()
        End If

        t = CSng(Timer)
        'MsgBox(Timer - loadTime)
        loadTime = Timer

        While (RenderWorld.GenerateFaceList.Count > 0)
            RenderWorld.UpdateChunks(blockData, allChanges, Randoms)
            If Timer - t > 0.1 Then
                t += CSng(0.1)
                Loading_Screen.LoadScreen(elapsedTime Mod 18)
                elapsedTime += 1
            End If
        End While

        'MsgBox(Timer - loadTime)

        'Zombie test
        For i = 0 To zombies.Length - 1
            zombies(i) = New Zombie(True)
        Next
        'End test

        Sound.PlaySound("Music")
        BackgroundColor = ConsoleColor.Black
        Clear()
        ForegroundColor = ConsoleColor.White
        WriteLine("Loaded in " & Timer - t)
        'Do 'TEMP HIDE EVERYTHING
        '    KeyboardInput.GetKeys()
        'Loop

        If TESTMODE Then
            Inventory.hotbar(0).itemID = 111
            Inventory.hotbar(0).numberOfItems = 1
            Inventory.hotbar(0).durability = 5
            Inventory.hotbar(1).itemID = 17
            Inventory.hotbar(1).numberOfItems = 5
            Inventory.hotbar(2).itemID = 20
            Inventory.hotbar(2).numberOfItems = 1
            Inventory.hotbar(3).itemID = 4
            Inventory.hotbar(3).numberOfItems = 10
            Inventory.hotbar(4).itemID = 105
            Inventory.hotbar(4).numberOfItems = 3
            Inventory.hotbar(4).itemID = 122
            Inventory.hotbar(4).numberOfItems = 1
            Inventory.hotbar(5).itemID = 18
            Inventory.hotbar(5).numberOfItems = 1
            Inventory.hotbar(6).itemID = 14
            Inventory.hotbar(6).numberOfItems = 20
            Inventory.hotbar(7).itemID = 11
            Inventory.hotbar(7).numberOfItems = 20
        End If

        If allFurnaces.Length < 10 Then
            ReDim Preserve allFurnaces(10)
            For j = 0 To allFurnaces.Length - 1
                If IsNothing(allFurnaces) Then
                    allFurnaces(j) = New Furnace()
                End If
            Next
        End If

        Clear()
        'SetOut(IO.TextWriter.Null)
        Window.Initialise()
        While Not Player.CanMoveThroughBlock(RenderWorld.GetBlock(CInt(Math.Floor(Player.x + Player.chunkX * 16)), CInt(Math.Floor(Player.y)), CInt(Math.Floor(Player.z + Player.chunkZ * 16))))
            Player.y += 0.5F
        End While

        oldTime = CSng(Timer)
        t = 0
        lastFurnaceUpdate = 0

        Do
            CursorVisible = False
            deltaTime = Math.Abs(deltaTime)
            t += deltaTime
            c += 1
            totalTime += deltaTime
            daytime = ((totalTime + LENGTHOFDAY * STARTTIME) Mod LENGTHOFDAY) / LENGTHOFDAY
            fps = CInt(c / (t + 0.0001))
            'SetCursorPosition(0, 0)
            'WriteLine(1000 / fps & " ms   ")
            'WriteLine(Math.Floor(timer1 / totalTime * 1000 / fps) & "  ")
            'WriteLine(Math.Floor(timer2 / totalTime * 1000 / fps) & "  ")
            'WriteLine(Math.Floor(timer3 / totalTime * 1000 / fps) & "  ")
            'WriteLine(Math.Floor(timer4 / totalTime * 1000 / fps) & "  ")
            If c Mod 100 = 0 Then c = 0 : t = 0

            key = KeyboardInput.GetKeys()
            If eToggle <> key.eToggle Then
                worldView = Not worldView
                If worldView And (currentInterface = Interfaces.Inventory Or currentInterface = Interfaces.Crafting) Then
                    For i = 0 To Inventory.crafting.GetLength(0) - 1
                        For j = 0 To Inventory.crafting.GetLength(1) - 1
                            While Inventory.crafting(i, j).numberOfItems > 0
                                Inventory.PickupItem(Inventory.crafting(i, j).itemID, soundData, itemData)
                                Inventory.crafting(i, j).numberOfItems -= CByte(1)
                            End While
                            Inventory.crafting(i, j).itemID = 0
                        Next
                    Next
                End If
            End If
            eToggle = key.eToggle

            deltaTime = CSng(Timer - oldTime)
            oldTime = CSng(Timer)

            If totalTime - lastFurnaceUpdate > 0.1 Then
                lastFurnaceUpdate += 0.1F
                UpdateAllFurnaces(fuelData, smeltData, allFurnaces, allChanges)
                If TrySpawnZombie(zombies(CInt(Rnd() * 19)), RenderWorld.DaytimeToDaylight(daytime)) Then 'CInt(Math.Floor(Rnd() * 20)))) Then
                    'SetCursorPosition(0, 0)
                    'WriteLine("ZOMBIE")
                End If
                If Player.health = 20 Then
                    lastHeal = 0
                End If
                lastHeal += 1
                If lastHeal = 40 Then
                    Player.health += 1
                    lastHeal = 0
                End If
            End If

            If worldView Then
                'timer2 -= CSng(Timer)
                currentInterface = Interfaces.Inventory
                Inventory.holding.itemID = CByte(Terrain.Blocks.air)

                Player.Rotate()
                Player.Jump()
                Player.Move(deltaTime, blockData, soundData)
                Player.ApplyGravity(deltaTime)
                Player.MineAndPlace(deltaTime, placeBlockToggle, attackToggle, currentInterface, blockData, itemData, allChanges, allFurnaces, furnaceIndex, zombies, soundData)
                If currentInterface = Interfaces.Crafting Or currentInterface = Interfaces.Furnace Then
                    worldView = False
                End If
                Inventory.ChangeSelection()

                UpdateAllZombies(zombies, deltaTime, soundData)
                'timer2 += CSng(Timer)
                RenderWorld.totFaces = 0
                RenderWorld.totFacesWater = 0
                'timer3 -= CSng(Timer)
                RenderWorld.UpdateChunks(blockData, allChanges, Randoms)
                'timer3 += CSng(Timer)
                'timer4 -= CSng(Timer)
                RenderWorld.RenderAllChunks(daytime, True)
                RenderWorld.RenderAllChunks(daytime, False)
                'timer4 += CSng(Timer)
                'timer1 -= CSng(Timer)
                RenderWorld.RenderWorld(daytime, blockData, zombies, openglData)
                Inventory.DisplayHotbar(blockData, itemData, Player.health, openglData)
                'timer1 += CSng(Timer)
            Else
                inventorySelected = Inventory.GetInventorySelected(currentInterface)
                oldMouse = Inventory.MoveItem(inventorySelected, oldMouse, currentInterface + 2, allFurnaces(furnaceIndex), itemData)
                Inventory.DisplayInventory(inventorySelected, inventoryCoords, inventoryTextures, blockData, itemData, inventoryColours)
                If currentInterface = Interfaces.Crafting Or currentInterface = Interfaces.Inventory Then
                    Inventory.DisplayCrafting(currentInterface + 2, inventorySelected, inventoryCoords, inventoryTextures, blockData, itemData, inventoryColours)
                Else
                    Inventory.DisplayFurnace(inventorySelected, inventoryCoords, inventoryTextures, blockData, itemData, allFurnaces(furnaceIndex), inventoryFraction, inventoryColours)
                End If
                Inventory.DisplaySelectedBlock(inventoryCoords, inventoryTextures, blockData, itemData, inventoryColours)
                Inventory.Display(inventoryCoords.ToArray(), inventoryTextures.ToArray(), inventoryFraction, inventoryColours.ToArray(), openglData)
                inventoryFraction.Clear()
            End If
            OpenGL.UpdateDisplay()
            If KeyboardInput.ShouldEndgame Then
                Endgame(allChanges, seed, saveGameName, allFurnaces, soundData)
            End If
            If Player.health <= 0 Then
                If GameOverScreen(openglData) Then
                    Respawn()
                Else
                    Endgame(allChanges, seed, saveGameName, allFurnaces, soundData)
                End If
            End If
        Loop
    End Sub

    Sub Respawn()
        For i = 0 To Inventory.hotbar.Length - 1
            Inventory.hotbar(i).numberOfItems = 0
            Inventory.hotbar(i).durability = 0
            Inventory.hotbar(i).itemID = 0
        Next
        For i = 0 To Inventory.inventory.Length - 1
            Inventory.inventory(i).numberOfItems = 0
            Inventory.inventory(i).durability = 0
            Inventory.inventory(i).itemID = 0
        Next
        Player.health = 20
    End Sub

    Function GameOverScreen(ByRef openGlData As OpenGL.OpenGlData) As Boolean
        Dim coords As New List(Of Single)
        Dim coordsOffsetX As Single() = {0, 0, 1, 1}
        Dim coordsOffsetY As Single() = {0, 1, 1, 0}
        Dim colours As New List(Of Byte)
        Dim textCoords As New List(Of Single)
        Dim textColours As New List(Of Byte)
        Dim textTextures As New List(Of Byte)

        For i = 0 To 3
            coords.Add(coordsOffsetX(i) * 2 - 1)
            coords.Add(coordsOffsetY(i) * 2 - 1)
            coords.Add(-1)
            coords.Add(1)
            colours.Add(127)
            colours.Add(0)
            colours.Add(0)
            colours.Add(200)
        Next
        OpenGL.InitTextures({CByte(RenderWorld.ZombieTextureStart - 1)}, RenderWorld.NumTexturesTotal, 1, New List(Of OpenGL.FractionalIcon), openGlData)
        OpenGL.RenderTransluscentBlock(coords.ToArray, colours.ToArray, openGlData, True)
        Inventory.WriteText(textCoords, textTextures, "Game over", textColours, 0.1F, 0.5F)
        Inventory.WriteText(textCoords, textTextures, "Press r to respawn", textColours, 0.05F, 0)
        Inventory.WriteText(textCoords, textTextures, "Or press x to exit", textColours, 0.05F, -0.1F)
        OpenGL.InitTextures(textTextures.ToArray(), RenderWorld.NumTexturesTotal, textTextures.Count, New List(Of OpenGL.FractionalIcon), openGlData)
        OpenGL.RenderGUI(textCoords.ToArray, textColours.ToArray, openGlData)
        OpenGL.UpdateDisplay()
        Do
            If KeyboardInput.ShouldEndgame Then
                Return False
            End If
            If KeyboardInput.ShouldRestart Then
                Return True
            End If
            OpenGL.UpdateDisplay()
        Loop
    End Function

    Function TrySpawnZombie(ByRef zombieData As Zombie, daylight As Single) As Boolean
        Dim x, y, z As Single
        If zombieData.inUse Then Return False
        x = Player.x + Player.chunkX * 16 - 50 + Rnd() * 100
        y = Rnd() * 250
        z = Player.z + Player.chunkZ * 16 - 50 + Rnd() * 100
        If zombieData.IsValidSpawn(x, y, z, daylight) Then
            zombieData.Initialise(x, y, z)
            Return True
        End If
        Return False
    End Function

    Sub NewGame(ByRef seed As Integer)
        Randomize()
        seed = CInt(Rnd() * 1000)
        'seed = 120
    End Sub

    Sub UpdateAllFurnaces(ByRef fuelData As ImportedData.FuelData(), ByRef smeltData As ImportedData.SmeltData(), ByRef furnaces As Furnace(), ByRef allChunkData As FEN.ChunkChanges())
        'For i = 0 To RenderWorld.LoadedChunks.Length - 1
        '    If RenderWorld.LoadedChunks(i).inUse Then
        '        For j = 0 To RenderWorld.LoadedChunks(i).numFurnaces - 1
        '            RenderWorld.LoadedChunks(i).furnaces(j).Tick(fuelData, smeltData)
        '        Next
        '    End If
        'Next
        For i = 0 To furnaces.Length - 1
            If Furnace.InRange(furnaces(i)) Then
                furnaces(i).Tick(fuelData, smeltData, allChunkData)
            End If
        Next
    End Sub

    Sub UpdateAllZombies(ByRef zombies As Zombie(), deltaTime As Single, ByRef soundData As Sound.SoundData)
        For i = 0 To zombies.Length - 1
            If zombies(i).inUse Then
                zombies(i).Move(deltaTime, Player.x + Player.chunkX * 16, Player.y, Player.chunkZ * 16 + Player.z, soundData)
                zombies(i).AttackPlayer(Player.health, Player.x + Player.chunkX * 16, Player.y, Player.z + Player.chunkZ * 16)
                'TEST
                'zombies(i).red = RayTracing.PointingAtCuboid(Player.playerAngle, Player.playerElevation, Player.x + Player.chunkX * 16, Player.y, Player.z + Player.chunkZ * 16, zombies(i).baseXCentre, zombies(i).baseYCentre, zombies(i).baseZCentre, zombies(i).orientation, 1, 2, 0.25F)
            End If
        Next
    End Sub
End Module

Public Enum Interfaces
    Inventory = 0
    Crafting = 1
    Furnace = 2
End Enum

Public Class Window

    Public Shared Sub Initialise()
        Dim bufferSize As COORD
        Dim consolePtr As IntPtr = GetForegroundWindow()
        Dim newSize As New COORD
        Dim fail As Boolean = False
        bufferSize.x = 10
        bufferSize.y = 10
        Clear()
        Do
            Try
                BufferHeight \= CInt(2)
            Catch
                fail = True
            End Try
        Loop Until fail Or BufferHeight < 50
        'ShowWindow(consolePtr, 3)
        SetConsoleDisplayMode(GetStdHandle(-11), 1, newSize)
        'SetWindowPos(consolePtr, 0, -100, -100, 2000, 2000, &H40)
        'MsgBox(SetConsoleScreenBufferSize(GetStdHandle(-10), bufferSize))
    End Sub

    Public Shared Function GetSize() As COORD
        Dim consolePtr As IntPtr = GetForegroundWindow()
        Dim coords As New RECT
        Dim size As New COORD
        GetWindowRect(consolePtr, coords)
        size.x = CShort(coords.r - coords.l)
        size.y = CShort(coords.b - coords.t)
        Return size
    End Function

    Public Shared Function GetPos() As COORD
        Dim consolePtr As IntPtr = GetForegroundWindow()
        Dim coords As New RECT
        Dim pos As New COORD
        GetWindowRect(consolePtr, coords)
        pos.x = CShort(coords.l)
        pos.y = CShort(coords.t)
        Return pos
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Structure RECT
        Dim l As Int32
        Dim t As Int32
        Dim r As Int32
        Dim b As Int32
    End Structure

    <DllImport("User32.dll")>
    Private Shared Function SetWindowPos(hnd As IntPtr, z As Int32, x As Int32, y As Int32, w As Int32, h As Int32, flags As UInt32) As Boolean
    End Function

    <DllImport("User32.dll")>
    Private Shared Function GetWindowRect(ByVal hnd As IntPtr, ByRef coords As RECT) As Boolean
    End Function

    <DllImport("Kernel32.dll")>
    Private Shared Function SetConsoleScreenBufferSize(hnd As IntPtr, size As COORD) As Boolean
    End Function

    <DllImport("Kernel32.dll")>
    Private Shared Function GetStdHandle(hnd As Int32) As IntPtr
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure COORD
        Dim x As Int16
        Dim y As Int16
    End Structure

    <DllImport("User32.dll")>
    Private Shared Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("User32.dll")>
    Private Shared Function ShowWindow(hnd As IntPtr, cmd As UInt32) As Boolean
    End Function

    <DllImport("Kernel32.dll")>
    Private Shared Function SetConsoleDisplayMode(hnd As IntPtr, cmd As UInt32, ByRef newSize As COORD) As Boolean
    End Function
End Class

Public Class Terrain

    Public Shared Sub GenerateChunk(chunkX As Integer, chunkZ As Integer, ByRef chunkPtr As Byte(), ByRef treeLocPtr As TreeLocation(), ByRef randoms As Single())
        Dim biome As Single
        Dim forest As Single
        Dim treeLoc As Byte
        Dim treeLocList As New List(Of TreeLocation)
        Dim randomValue As Integer
        Dim randomValueSng As Single
        Dim x, y, z As Integer
        Dim xa, za As Integer
        Dim oldRnd As Single
        Dim surface As Byte
        Dim result(16, 256, 16) As Byte
        forest = CSng(Random.Perlin2D(chunkX, chunkZ, 20, 0.75, randoms) ^ 5)
        If Not IsNothing(chunkPtr) Then Array.Clear(chunkPtr, 0, chunkPtr.Length)
        For x = 0 To 15
            For z = 0 To 15
                xa = x + chunkX * 16
                za = z + chunkZ * 16
                biome = Random.Perlin2D(xa, za, 501, 10, randoms)
                treeLoc = CByte(CInt(Math.Floor(forest)) \ 16)

                randomValueSng = 0
                randomValueSng += Random.Perlin2D(xa, za, 301, 127, randoms)
                randomValueSng += Random.Perlin2D(xa, za, 101, 40, randoms)
                randomValueSng += Random.Perlin2D(xa, za, 31, 10, randoms)
                randomValueSng += Random.Perlin2D(xa, za, 13, 7, randoms)
                oldRnd = randomValue
                If biome < 3 Then
                    randomValueSng += Random.Perlin2D(xa, za, 31, 21, randoms) * (3 - biome)
                    randomValueSng += Random.Perlin2D(xa, za, 13, 8, randoms) * (3 - biome)
                End If
                randomValue = CInt(randomValueSng)

                For y = 0 To 255
                    result(x, y, z) = CByte(Blocks.air)
                Next

                For y = 0 To randomValue
                    result(x, y, z) = CByte(Blocks.stone)
                Next


                If randomValue <= 60 Then
                    For y = randomValue To 60
                        result(x, y, z) = CByte(Blocks.water)
                    Next
                Else
                    For y = randomValue - 5 To randomValue - 1
                        result(x, y, z) = CByte(Blocks.dirt)
                    Next
                    result(x, randomValue, z) = CByte(Blocks.grass)
                End If

                If biome + oldRnd / 100 < 3 Then
                    For y = 0 To randomValue
                        result(x, y, z) = CByte(Blocks.stone)
                    Next
                End If

                result(x, 0, z) = CByte(Blocks.bedrock)

                surface = 255
                While result(x, surface, z) = 0
                    surface -= CByte(1)
                End While
                If Random.PseudoRandom(xa * 100 + za * 10000, randoms) < forest And result(x, randomValue, z) = Blocks.grass Then
                    'result(x, randomValue + 1, z) = Blocks.treeSeed
                    treeLocList.Add(New TreeLocation(x, randomValue + 1, z))
                    result(x, randomValue, z) = CByte(Blocks.dirt)
                End If
            Next
        Next
        ReDim chunkPtr(65536)
        For z = 0 To 15
            For y = 0 To 255
                For x = 0 To 15
                    chunkPtr(z * 4096 + y * 16 + x) = result(x, y, z)
                    'Marshal.WriteByte(chunkPtr + z * 256 * 16 + y * 16 + x, result(x, y, z))
                Next
            Next
        Next
        treeLocPtr = treeLocList.ToArray()
    End Sub

    Public Structure TreeLocation
        Public x As Integer
        Public y As Integer
        Public z As Integer
        Sub New(xIn As Integer, yIn As Integer, zIn As Integer)
            x = xIn
            y = yIn
            z = zIn
        End Sub
    End Structure

    Public Shared Sub PostGeneration(ByRef chunkPtr As Byte(), surroundLists As TreeLocation(,)(), ByVal oreSeed As Integer, ByRef finalChanges As List(Of FEN.BlockChange), ByRef randoms As Single())
        Dim numOre As Integer = 0
        Dim randomCounter As Integer = 0
        For i = 0 To 2
            For j = 0 To 2
                For k = 0 To surroundLists(i, j).Length - 1
                    DrawTree(chunkPtr, surroundLists(i, j)(k), i - 1, j - 1)
                Next
            Next
        Next
        GenerateOre(oreSeed, randomCounter, chunkPtr, Blocks.coalOre, 50, 20, 255, randoms)
        GenerateOre(oreSeed, randomCounter, chunkPtr, Blocks.ironOre, 30, 10, 128, randoms)
        GenerateOre(oreSeed, randomCounter, chunkPtr, Blocks.goldOre, 25, 5, 64, randoms)
        GenerateOre(oreSeed, randomCounter, chunkPtr, Blocks.diamondOre, 15, 3, 32, randoms)
        For i = 0 To finalChanges.Count - 1
            chunkPtr(finalChanges(i).coord) = finalChanges(i).blockID
        Next
    End Sub

    Private Shared Function GenerateOre(oreLoc As Integer, ByRef randomCounter As Integer, ByRef chunkPtr As Byte(), oreType As Blocks, numVeins As Integer, sizeOfVein As Integer, height As Byte, ByRef randoms As Single()) As Integer
        Dim direction As Integer() = {-1, 1, -16, 16, -4096, 4096}
        Dim numOre As Integer
        Dim heightMask As Integer = &HFF0
        For i = 1 To numVeins
            oreLoc = CInt(Random.PseudoRandom(oreLoc + randomCounter, randoms) * 65535)
            For j = 1 To sizeOfVein
                randomCounter += 1
                oreLoc += direction(CInt(Math.Floor(Random.PseudoRandom(oreLoc + randomCounter, randoms) * 5)))
                oreLoc += 65536
                oreLoc = oreLoc Mod 65536
                If chunkPtr(oreLoc) = Blocks.stone And ((oreLoc And heightMask) < height * 16) Then
                    chunkPtr(oreLoc) = CByte(oreType)
                    numOre += 1
                End If
            Next
        Next
        Return numOre
    End Function

    Private Shared Sub DrawTree(ByRef data As Byte(), treeLoc As TreeLocation, offsetX As Integer, offsetZ As Integer)
        Dim xa As Integer = treeLoc.x + offsetX * 16
        Dim ya As Integer = treeLoc.y
        Dim za As Integer = treeLoc.z + offsetZ * 16
        For i = -2 To 2
            For k = -2 To 2
                For j = 3 To 4
                    TryDrawBlock(data, xa + k, ya + j, za + i, CByte(Blocks.leaf))
                Next
            Next
        Next
        For i = 0 To 5
            TryDrawBlock(data, xa, ya + i, za, CByte(Blocks.log))
            'chunkPtr(z * 4096 + (y + i) * 16 + x) = Blocks.log
        Next
        'Change this so it alters different chunks
        TryDrawBlock(data, xa, ya + 6, za, CByte(Blocks.leaf))
        TryDrawBlock(data, xa - 1, ya + 5, za, CByte(Blocks.leaf))
        TryDrawBlock(data, xa + 1, ya + 5, za, CByte(Blocks.leaf))
        TryDrawBlock(data, xa, ya + 5, za - 1, CByte(Blocks.leaf))
        TryDrawBlock(data, xa, ya + 5, za + 1, CByte(Blocks.leaf))
    End Sub

    Private Shared Sub TryDrawBlock(ByRef data As Byte(), x As Integer, y As Integer, z As Integer, blockId As Byte)
        If x >= 0 And x <= 15 And z >= 0 And z <= 15 Then
            If data(x + y * 16 + z * 4096) = Blocks.air Or data(x + y * 16 + z * 4096) = Blocks.leaf Then
                data(x + y * 16 + z * 4096) = blockId
            End If
        End If
    End Sub

    Public Enum Blocks
        air = 0
        stone = 1
        grass = 2
        dirt = 3
        bedrock = 6
        water = 7
        log = 14
        leaf = 15
        torch = 17
        furnace = 18
        craftingTable = 20
        coalOre = 13
        ironOre = 12
        goldOre = 11
        diamondOre = 16
        treeSeed = 255
    End Enum

    Private Shared Function Rescale(value As Single, scale As Single) As Single
        If scale * value > 255 Then Return 255
        If scale * value < 0 Then Return 0
        Return scale * value
    End Function
End Class

Public Class Random
    'Public Shared Randoms(100000) As Single

    Public Shared Function PseudoRandom(input As Integer, ByRef Randoms As Single()) As Single
        While input < 0
            input += Randoms.Length
        End While
        Return Randoms(input Mod Randoms.Length)
    End Function

    Public Shared Sub Initialise(seed As Integer, ByRef Randoms As Single())
        Dim rnd As New System.Random(seed)
        For i = 0 To Randoms.Length - 1
            Randoms(i) = CSng(rnd.NextDouble)
        Next
        max = 0
        min = 10
    End Sub

    Public Shared max As Single
    Public Shared min As Single

    Public Shared Function Noise(input As Single, length As Single, amplitude As Single, ByRef randoms As Single()) As Single
        Dim a, b As Integer
        Dim rndA, rndB As Single
        Dim rndPosA, rndPosB As Single
        Dim fraction As Single
        Dim interpolate As Single
        a = CInt(Int(input / length))
        b = a + 1
        fraction = (input / length) - (Int(input / length))
        fraction = CSng(Math.Cos(fraction * Math.PI) * -1 + 1) / 2
        rndA = PseudoRandom(a, randoms)
        rndB = PseudoRandom(b, randoms)
        rndPosA = CSng((rndA - 0.5) * fraction)
        rndPosB = CSng((rndB - 0.5) * (1 - fraction))
        interpolate = CSng((rndPosA * (1 - fraction) + rndPosB * fraction) + 0.5)
        Return interpolate * amplitude
    End Function

    Public Shared Function Perlin2D(inputX As Single, inputY As Single, length As Single, amplitude As Single, ByRef randoms As Single()) As Single
        inputX += CSng(0.5)
        inputY += CSng(0.5)
        Dim coordBeforeX As Single = (Int(inputX / length))
        Dim coordBeforeY As Single = (Int(inputY / length))
        Dim interpolateX As Single = InterpolateFraction(inputX, length)
        Dim interpolateY As Single = InterpolateFraction(inputY, length)
        Dim rndDirection As Single
        Dim theta As Single
        Dim dot As Single
        Dim dots(3) As Single
        Dim returnValue As Single
        Dim closest As Single = 10
        Dim correction As Single() = {-1, 1}

        For i = 0 To 1
            For j = 0 To 1
                rndDirection = CSng(PseudoRandom(CInt((coordBeforeX + i) + (coordBeforeY + j) * 5000), randoms) * Math.PI * 2)
                If interpolateX = 0 Then
                    theta = 90
                Else
                    theta = CSng(Math.Atan(interpolateY / interpolateX))
                End If
                dot = CSng(Math.Cos(theta + rndDirection * correction((j + i) Mod 2)) * Distance(interpolateX, interpolateY))
                If i = 1 Then dot *= -1
                dots(i + j * 2) = CSng((dot + 0.71) / 1.5)
                'If Distance(interpolateX, interpolateY) < closest Then
                '    closest = Distance(interpolateX, interpolateY)
                '    closestDot = j * 2 + i
                'End If
                interpolateY = 1 - interpolateY
                'dots(j * 2 + i) = PseudoRandom((coordBeforeX + i) + (coordBeforeY + j) * 10000)

            Next
            interpolateX = 1 - interpolateX
        Next
        returnValue = Interpolate2D(dots, interpolateX, interpolateY)
        If returnValue > max Then max = returnValue
        If returnValue < min Then min = returnValue
        Return returnValue * amplitude
    End Function

    Public Shared Function Noise2D(inputX As Single, inputY As Single, length As Single, amplitude As Single, ByRef randoms As Single()) As Single
        Dim coordBeforeX As Single = (Int(inputX / length))
        Dim coordBeforeY As Single = (Int(inputY / length))
        Dim coordOffsetX, coordOffsetY As Single
        Dim interpolateX As Single = InterpolateFraction(inputX, length)
        Dim interpolateY As Single = InterpolateFraction(inputY, length)
        Dim peakLocX(2, 2), peakLocY(2, 2) As Single
        Dim minI As Single = 0
        Dim minD As Single = 1000
        Dim min2I As Single = 0
        For i = 0 To 2
            For j = 0 To 2
                coordOffsetX = coordBeforeX + i - 1
                coordOffsetY = coordBeforeY + j - 1
                peakLocX(i, j) = PseudoRandom(CInt(coordOffsetX + coordOffsetY * 10000), randoms) + i - 1
                peakLocY(i, j) = PseudoRandom(CInt(coordOffsetX * 10000 + coordOffsetY + 10000), randoms) + j - 1
                If minD > Distance(peakLocX(i, j) - interpolateX, peakLocY(i, j) - interpolateY) / 1.5 Then
                    minD = Distance(peakLocX(i, j) - interpolateX, peakLocY(i, j) - interpolateY) / 1.5F
                    min2I = minI
                    minI = i * 3 + j
                End If
            Next
        Next
        Return Interpolate({peakLocX(CInt(minI) \ 3, CInt(minI) Mod 3), peakLocX(CInt(min2I) \ 3, CInt(min2I) Mod 3)}, {peakLocY(CInt(minI) \ 3, CInt(minI) Mod 3), peakLocY(CInt(min2I) \ 3, CInt(min2I) Mod 3)}, {1, 0}, interpolateX, interpolateY)
        coordOffsetX = 1
        If peakLocX(0, 0) > 0 Then
            coordOffsetX = -1
        End If
        coordOffsetY = 1
        If peakLocY(0, 0) > 0 Then
            coordOffsetY = -1
        End If
        Return minD
    End Function

    Public Shared Function Interpolate2D(data As Single(), x As Single, y As Single) As Single
        Dim data1() As Single = {Interpolate1D({data(0), data(1)}, x), Interpolate1D({data(2), data(3)}, x)}
        Return Interpolate1D(data1, y)
    End Function

    Public Shared Function Interpolate1D(data As Single(), fraction As Single) As Single
        Return data(0) * (1 - fraction) + data(1) * fraction
    End Function

    Public Shared Function Interpolate(x As Single(), y As Single(), values As Single(), xMain As Single, yMain As Single) As Single
        Dim inverseDistance(x.Length - 1) As Single
        Dim totalInverseDistance As Single = 0
        Dim totalValues As Single = 0
        For i = 0 To x.Length - 1
            inverseDistance(i) = 1 / Distance(x(i) - xMain, y(i) - yMain)
            totalInverseDistance += inverseDistance(i)
            totalValues += inverseDistance(i) * values(i)
        Next
        Return totalValues / totalInverseDistance
    End Function

    Public Shared Function Distance(x As Single, y As Single) As Single
        If x = 0 And y = 0 Then Return 0.001
        Return CSng((x ^ 2 + y ^ 2) ^ 0.5)
    End Function

    Public Shared Function Noise1D(inputX As Single, length As Single, amplitude As Single, ByRef randoms As Single()) As Single
        Dim coordBefore As Single = (Int(inputX / length))
        Dim rndBefore As Single = PseudoRandom(CInt(coordBefore), randoms)
        Dim rndAfter As Single = PseudoRandom(CInt(coordBefore) + 1, randoms)
        Return Interpolate(rndBefore, rndAfter, InterpolateFraction(inputX, length)) * amplitude
    End Function

    Public Shared Function InterpolateFraction(coords As Single, length As Single) As Single
        Dim linear As Single = coords / length - Int(coords / length)
        Return linear
        Return CSng((Math.Cos(linear * Math.PI) * -1 + 1) / 2)
    End Function
    Private Shared Function Interpolate(a As Single, b As Single, shift As Single) As Single
        Return (1 - shift) * a + shift * b
    End Function
End Class

Public Class MouseInput
    Public Shared Sub Initialise()
        Dim hnd As IntPtr = GetStdHandle(-10)
        Dim mode As Int32
        GetConsoleMode(hnd, mode)
        SetConsoleMode(hnd, mode And Not 64)
        HideMouseCursor()
    End Sub

    Private Shared Sub HideMouseCursor()
        SystemParametersInfoA(&H2029, 0, 1, 0)
    End Sub

    Public Shared Sub ShowMouseCursor()
        SystemParametersInfoA(&H2029, 0, 32, 0)
    End Sub

    Public Shared Function GetInput() As MouseInput
        Dim mouseState As New MouseInput
        Dim coords As New POINT
        mouseState.left = GetKeyState(1) < 0
        mouseState.right = GetKeyState(2) < 0
        mouseState.leftToggle = CBool(GetKeyState(1) Mod 2)
        mouseState.rightToggle = CBool(GetKeyState(2) Mod 2)
        GetCursorPos(coords)
        mouseState.x = coords.x
        mouseState.y = coords.y
        Return mouseState
    End Function

    Public Shared Function GetRelativeLoc(absX As Integer, absY As Integer) As Single()
        Dim windowSize As Window.COORD = Window.GetSize()
        Dim windowPos As Window.COORD = Window.GetPos()
        Return {CSng(2 * (absX - windowPos.x) / windowSize.x - 1), CSng(2 * (windowPos.y - absY) / windowSize.y + 1)}
    End Function

    Public Shared Function GetRelativeLocRescaled(relativeLoc As Single()) As Single()
        Dim size As Window.COORD = Window.GetSize()
        Return {relativeLoc(0) * size.x / size.y, relativeLoc(1)}
    End Function

    Public Shared Sub ForceMouseMove(x As Int32, y As Int32)
        SetCursorPos(x, y)
    End Sub

    Structure MouseInput
        Public left As Boolean
        Public right As Boolean
        Public leftToggle As Boolean
        Public rightToggle As Boolean
        Public x As Int32
        Public y As Int32
    End Structure

    Private Structure POINT
        Public x As Int32
        Public y As Int32
    End Structure

    <DllImport("User32.dll")>
    Private Shared Function ShowCursor(ByVal action As Boolean) As Int32
    End Function

    <DllImport("User32.dll")>
    Private Shared Function GetCursorPos(ByRef p As POINT) As Boolean
    End Function

    <DllImport("User32.dll")>
    Private Shared Function SetCursorPos(ByVal x As Int32, ByVal y As Int32) As Boolean
    End Function

    <DllImport("User32.dll")>
    Private Shared Function GetKeyState(key As Int32) As Int16
    End Function

    <DllImport("Kernel32.dll")>
    Private Shared Function SetConsoleMode(hnd As IntPtr, mode As Int32) As Boolean
    End Function
    <DllImport("Kernel32.dll")>
    Private Shared Function GetConsoleMode(ByVal hnd As IntPtr, ByRef mode As Int32) As Boolean
    End Function
    <DllImport("Kernel32.dll")>
    Private Shared Function GetStdHandle(hnd As Int32) As IntPtr
    End Function
    <DllImport("User32.dll")>
    Private Shared Function SystemParametersInfoA(action As UInt32, param As UInt32, param2 As UInt32, ini As UInt32) As Boolean
    End Function

End Class

Public Class Loading_Screen
    Public Shared Sub LoadScreenInit()
        BackgroundColor = ConsoleColor.Blue
        Clear()
        SetCursorPosition(10, 50) : ForegroundColor = ConsoleColor.White
        WriteLine("GENERATING TERRAIN")
    End Sub
    Public Shared Sub LoadScreen(blockChange As Integer)
        Dim x As Int32() = {10, 19, 19, 28, 28, 28, 37, 37, 46}
        Dim y As Int32() = {28, 28, 19, 28, 19, 10, 28, 19, 28}
        Dim pulse As String() = {".        ", ". .       ", ". . .     ", ". . . .    ", ". . . . . "}
        CursorVisible = False
        If blockChange < 9 Then
            choose(x(blockChange), y(blockChange))
            If blockChange Mod 2 = 0 Then
                SetCursorPosition(29, 50) : ForegroundColor = ConsoleColor.White
                WriteLine(pulse(blockChange \ 2))
            End If
        Else
            clearBlock(x(blockChange - 9), y(blockChange - 9))
        End If
    End Sub
    Private Shared Sub choose(left As Integer, top As Integer)
        Dim rand As Integer = random(16)
        If rand = 0 Then stone(left, top)
        If rand = 1 Then colourore(left, top, 3)
        If rand = 2 Then colourore(left, top, 0)
        If rand = 3 Then colourore(left, top, 1)
        If rand = 4 Then colourore(left, top, 2)
        If rand = 5 Or rand = 6 Or rand = 7 Then sand(left, top)
        If rand > 7 Then stone(left, top)
    End Sub
    Private Shared Function random(number As Integer) As Integer
        Randomize()
        Return CInt(Rnd() * number)
    End Function
    Private Shared Sub stone(left As Integer, top As Integer)
        For i = 0 To 7
            SetCursorPosition(left, top + i)
            ForegroundColor = ConsoleColor.DarkGray : WriteLine("████████")
        Next
    End Sub
    Private Shared Sub clearBlock(left As Integer, top As Integer)
        For i = 0 To 7
            SetCursorPosition(left, top + i)
            ForegroundColor = ConsoleColor.Blue : WriteLine("████████")
        Next
    End Sub
    Private Shared Sub sand(Left As Integer, top As Integer)
        For i = 0 To 7
            SetCursorPosition(Left, top + i)
            ForegroundColor = ConsoleColor.Yellow : WriteLine("████████")
        Next
    End Sub
    Private Shared Sub colourore(left As Integer, top As Integer, type As Integer)
        ore(left, top)
        If type = 0 Then ForegroundColor = ConsoleColor.DarkYellow
        If type = 1 Then ForegroundColor = ConsoleColor.Yellow
        If type = 2 Then ForegroundColor = ConsoleColor.Cyan
        If type = 3 Then ForegroundColor = ConsoleColor.Black
        SetCursorPosition(left + 2, top + 1) : Write("█")
        SetCursorPosition(left + 5, top + 1) : Write("█")
        SetCursorPosition(left + 4, top + 3) : Write("█")
        SetCursorPosition(left + 1, top + 4) : Write("█")
        SetCursorPosition(left + 6, top + 4) : Write("█")
        SetCursorPosition(left + 3, top + 5) : Write("█")
        SetCursorPosition(left + 2, top + 6) : Write("█")
        SetCursorPosition(left + 5, top + 6) : Write("█")
        WriteLine()
    End Sub
    Private Shared Sub ore(left As Integer, top As Integer)
        Dim coal() As String = {"████████", "██ ██ ██", "████████", "████ ███", "█ ████ █", "███ ████", "██ ██ ██", "████████"}
        For i = 0 To 7
            SetCursorPosition(left, top + i)
            ForegroundColor = ConsoleColor.DarkGray : WriteLine(coal(i))
        Next
    End Sub
End Class

Public Class KeyboardInput
    Private Enum KeyCode
        W = &H57
        A = &H41
        S = &H53
        D = &H44
        E = &H45
        R = &H52
        X = &H58
        Space = 32
        SHIFT = 16
        NUMBER = 48
    End Enum

    Public Shared Function ShouldEndgame() As Boolean
        Return GetKeyState(KeyCode.X) < 0
    End Function

    Public Shared Function ShouldRestart() As Boolean
        Return GetKeyState(KeyCode.R) < 0
    End Function

    Public Shared Function GetKeys() As Keys
        Dim states As New Keys
        Dim numbers(9) As Boolean
        states.w = GetKeyState(KeyCode.W) < 0
        states.a = GetKeyState(KeyCode.A) < 0
        states.s = GetKeyState(KeyCode.S) < 0
        states.d = GetKeyState(KeyCode.D) < 0
        states.e = GetKeyState(KeyCode.E) < 0
        states.eToggle = (GetKeyState(KeyCode.E) Mod 2) = 1
        states.space = GetKeyState(KeyCode.Space) < 0
        states.shift = GetKeyState(KeyCode.SHIFT) < 0
        For i = 0 To 9
            numbers(i) = GetKeyState(KeyCode.NUMBER + i) < 0
        Next
        states.numbers = numbers
        Return states
    End Function

    Public Structure Keys
        Public w As Boolean
        Public a As Boolean
        Public s As Boolean
        Public d As Boolean
        Public e As Boolean
        Public eToggle As Boolean
        Public numbers As Boolean()
        Public space As Boolean
        Public shift As Boolean
    End Structure

    <DllImport("User32.dll")>
    Private Shared Function GetKeyState(key As Int32) As Int16
    End Function
End Class

Public Class ImportedData
    Public Structure BlockData
        Public ID As Byte
        Public Name As String
        Public DropID As Byte
        Public Hardness As Single
        Public Pickaxe As Boolean
        Public Axe As Boolean
        Public Shovel As Boolean
        Public MinToolLevel As Integer
        Public Faces As String()
        Public FacesIndex As Byte()
        Public UniqueFaces As String()
        Public Sound As String

        Public Sub New(data As String)
            Dim splitData As String() = data.Split(","c)
            ReDim Faces(5)
            ReDim FacesIndex(5)
            ID = CByte(Val(splitData(0)))
            Name = splitData(1)
            DropID = CByte(Val(splitData(2)))
            Hardness = CSng(Val(splitData(3)))
            Pickaxe = splitData(4) <> "0"
            Axe = splitData(5) <> "0"
            Shovel = splitData(6) <> "0"
            MinToolLevel = CInt(Val(splitData(7)))
            For i = 0 To 5
                Faces(i) = splitData(8 + i)
            Next
            Sound = splitData(14)
        End Sub

        Public Function GenerateIndex(offset As Integer) As Integer
            Dim usedFaces As New List(Of String)
            For i = 0 To 5
                If Not usedFaces.Contains(Faces(i)) Then
                    FacesIndex(i) = CByte(usedFaces.Count + offset)
                    usedFaces.Add(Faces(i))
                Else
                    FacesIndex(i) = CByte(usedFaces.IndexOf(Faces(i)) + offset)
                End If
            Next
            UniqueFaces = usedFaces.ToArray()
            Return usedFaces.Count
        End Function
    End Structure

    Public Structure ItemData
        Public ID As Integer
        Public Name As String
        Public Picaxe As Integer
        Public Axe As Integer
        Public Shovel As Integer
        Public Sword As Integer
        Public TextureIndex As Integer
        Public Durability As Integer

        Public Sub New(data As String)
            Dim splitData As String() = data.Split(","c)
            ID = CInt(Val(splitData(0)) + 100)
            Name = splitData(1)
            Picaxe = CInt(Val(splitData(2)))
            Axe = CInt(Val(splitData(3)))
            Shovel = CInt(Val(splitData(4)))
            Sword = CInt(Val(splitData(5)))
            Durability = CInt(Val(splitData(6)))
        End Sub
    End Structure

    Public Shared Function ImportBlockData(path As String) As BlockData()
        Dim rawData As String() = IO.File.ReadAllLines("Resource\" & path & ".txt")
        Dim blockData(CInt(rawData(0))) As BlockData
        For i = 2 To rawData.Length - 1
            blockData(CInt(Val(rawData(i).Split(","c)(0)))) = New BlockData(rawData(i))
        Next
        For i = 0 To blockData.Length - 1
            If IsNothing(blockData(i).Name) Then blockData(i) = New BlockData(rawData(2))
        Next
        Return blockData
    End Function

    Public Shared Function ImportItemData(path As String) As ItemData()
        Dim rawData As String() = IO.File.ReadAllLines("Resource\" & path & ".txt")
        Dim itemData(CInt(rawData(0))) As ItemData
        For i = 2 To rawData.Length - 1
            itemData(CInt(Val(rawData(i).Split(","c)(0)))) = New ItemData(rawData(i))
        Next
        For i = 0 To itemData.Length - 1
            If IsNothing(itemData(i).Name) Then itemData(i) = New ItemData(rawData(2))
        Next
        Return itemData
    End Function

    Public Shared Function ImportFuelData(path As String) As FuelData()
        Dim rawData As String() = IO.File.ReadAllLines("Resource\" & path & ".txt")
        Dim fuelData(rawData.Length - 1) As FuelData
        For i = 1 To fuelData.Length - 1
            fuelData(i).ID = CByte(Val(rawData(i).Split(","c)(0)))
            fuelData(i).burnTime = CInt(Val(rawData(i).Split(","c)(1)))
        Next
        Return fuelData
    End Function

    Public Shared Function ImportSmeltData(path As String) As SmeltData()
        Dim rawData As String() = IO.File.ReadAllLines("Resource\" & path & ".txt")
        Dim smeltData(rawData.Length - 1) As SmeltData
        For i = 1 To smeltData.Length - 1
            smeltData(i).ID = CByte(Val(rawData(i).Split(","c)(0)))
            smeltData(i).outputID = CByte(Val(rawData(i).Split(","c)(1)))
            smeltData(i).smeltTime = CInt(Val(rawData(i).Split(","c)(2)))
        Next
        Return smeltData
    End Function

    Public Structure FuelData
        Public ID As Byte
        Public burnTime As Integer
    End Structure

    Public Structure SmeltData
        Public ID As Byte
        Public outputID As Byte
        Public smeltTime As Integer
    End Structure
End Class

Public Class EmbedData
    Public Shared Sub OutputData(data As String)
        Dim splitFolder As String() = data.Split("!"c)(0).Split(";"c)
        Dim splitData As String() = data.Split("!"c)(1).Split(";"c)
        Dim newFolder As String
        If My.Application.Info.DirectoryPath.Length > 80 Then
            newFolder = My.Application.Info.DirectoryPath
            While newFolder.Length > 80
                newFolder = newFolder.Substring(0, newFolder.Length - 1)
                While newFolder(newFolder.Length - 1) <> "\"c
                    newFolder = newFolder.Substring(0, newFolder.Length - 1)
                End While
                SetCurrentDirectory(Sound.StrToPtr(newFolder))
            End While
        End If
        For i = 0 To splitFolder.Length - 1
            Try
                IO.Directory.CreateDirectory(splitFolder(i))
            Catch
            End Try
        Next
        For i = 0 To splitData.Length - 2 Step 2
            Try
                If Not splitData(i).Contains("Resource") Then
                    splitData(i) = "Resource\" & splitData(i)
                End If
                IO.File.WriteAllBytes(splitData(i), Decode(splitData(i + 1)))
            Catch
                i = i
            End Try
        Next
    End Sub

    Private Shared Function Decode(data As String) As Byte()
        Dim dataByte As New List(Of Byte)
        Dim dataByteOne As Byte
        For i = 0 To data.Length - 2 Step 2
            dataByteOne = CByte(Asc(data(i)) - Asc("A"))
            dataByteOne *= CByte(16)
            dataByteOne += CByte(Asc(data(i + 1)) - Asc("A"))
            dataByte.Add(dataByteOne)
        Next
        Return dataByte.ToArray
    End Function

    Public Shared Function DataToString() As String
        Dim data As New Text.StringBuilder
        Dim fileData As Byte()
        Dim fileDataEncoded As String
        Dim files As String() = GetAllFiles("Resource")
        Dim folders As String() = GetAllDirectories("Resource")
        For i = 0 To folders.Length - 1
            For j = 0 To folders(i).Length - 1
                data.Append(folders(i)(j))
            Next
            data.Append(";")
        Next
        data.Append("!")
        For i = 0 To files.Length - 1
            fileData = IO.File.ReadAllBytes(files(i))
            fileDataEncoded = BinaryToString(fileData)
            For j = 0 To files(i).Length - 1
                data.Append(files(i)(j))
            Next
            data.Append(";")
            For j = 0 To fileDataEncoded.Length - 1
                data.Append(fileDataEncoded(j))
            Next
            data.Append(";")
        Next
        Return data.ToString
    End Function

    Private Shared Function BinaryToString(inputData As Byte()) As String
        Dim encoded As New Text.StringBuilder
        For i = 0 To inputData.Length - 1
            encoded.Append(Chr(inputData(i) \ 16 + Asc("A")))
            encoded.Append(Chr(inputData(i) Mod 16 + Asc("A")))
        Next
        Return encoded.ToString()
    End Function

    Private Shared Function GetAllDirectories(path As String) As String()
        Dim paths As New List(Of String)
        Dim directories As New List(Of String)
        directories.AddRange(IO.Directory.GetDirectories(path))
        paths.AddRange(IO.Directory.GetDirectories(path))
        While directories.Count > 0
            paths.AddRange(IO.Directory.GetDirectories(directories(0)))
            directories.AddRange(IO.Directory.GetDirectories(directories(0)))
            directories.RemoveAt(0)
        End While
        Return paths.ToArray()
    End Function

    Private Shared Function GetAllFiles(path As String) As String()
        Dim paths As New List(Of String)
        Dim directories As New List(Of String)
        directories.AddRange(IO.Directory.GetDirectories(path))
        paths.AddRange(IO.Directory.GetFiles(path))
        While directories.Count > 0
            paths.AddRange(IO.Directory.GetFiles(directories(0)))
            directories.AddRange(IO.Directory.GetDirectories(directories(0)))
            directories.RemoveAt(0)
        End While
        Return paths.ToArray()
    End Function


    <DllImport("kernel32.dll")>
    Private Shared Function SetCurrentDirectory(dir As IntPtr) As Boolean
    End Function

End Class

Public Class CustomBlocks
    Public Shared Sub Torch(ByVal attached As Integer, ByVal offset As COORD3, ByRef coordData As List(Of Single), ByRef faceData As List(Of Byte), ByRef lightData As List(Of Byte), ByRef blocks As ImportedData.BlockData())
        Dim baseCoordsOfModel As New List(Of Single)
        Dim rot As New COORD3
        Dim location As New COORD3
        location.x = 0
        location.y = 0
        location.z = 0
        For i = 0 To 3
            faceData.Add(blocks(Terrain.Blocks.torch).FacesIndex(i + 2))
        Next
        faceData.Add(blocks(Terrain.Blocks.torch).FacesIndex(0))
        location.x += 7 / 16.0F
        XPlane(location, baseCoordsOfModel)
        location.x += 1 / 8.0F
        XPlane(location, baseCoordsOfModel)
        location.x -= 9 / 16.0F
        location.z += 7 / 16.0F
        ZPlane(location, baseCoordsOfModel)
        location.z += 1 / 8.0F
        ZPlane(location, baseCoordsOfModel)
        location.z -= 9 / 16.0F
        location.y += 5 / 8.0F
        YPLane(location, baseCoordsOfModel)
        Select Case attached
            Case TorchBlockAttached.down
                rot.x = 0
                rot.z = 0
            Case TorchBlockAttached.right
                rot.x = 0.5
                offset.x += 1 - CSng(Math.Cos(0.5)) * 8 / 16
                offset.z += 1 / 16.0F
            Case TorchBlockAttached.forward
                rot.z = 0.5
                offset.z += 1 - CSng(Math.Cos(0.5)) * 8 / 16
                offset.x += 1 / 16.0F
            Case TorchBlockAttached.left
                rot.x = -0.5
                offset.x -= CSng(Math.Cos(0.5)) * 10 / 16
                offset.y += 2 * CSng(Math.Sin(0.5)) * 8 / 16
                offset.z += 1 / 16.0F
            Case TorchBlockAttached.backward
                rot.z = -0.5
                offset.z -= CSng(Math.Cos(0.5)) * 10 / 16
                offset.y += 2 * CSng(Math.Sin(0.5)) * 8 / 16
                offset.x += 1 / 16.0F
        End Select
        For i = 0 To baseCoordsOfModel.Count - 1 Step 4
            coordData.Add(baseCoordsOfModel(i) * CSng(Math.Cos(rot.z)) - baseCoordsOfModel(i + 1) * CSng(Math.Sin(rot.x)) + offset.x)
            coordData.Add(baseCoordsOfModel(i + 1) * CSng(Math.Cos(rot.z + rot.x)) + baseCoordsOfModel(i + 2) * CSng(Math.Sin(rot.z)) + baseCoordsOfModel(i) * CSng(Math.Sin(rot.x)) + offset.y)
            coordData.Add(baseCoordsOfModel(i + 2) * CSng(Math.Cos(rot.x)) - baseCoordsOfModel(i + 1) * CSng(Math.Sin(rot.z)) + offset.z)
            coordData.Add(baseCoordsOfModel(i + 3))
        Next
    End Sub

    Private Shared Sub YPLane(ByVal start As COORD3, ByRef data As List(Of Single))
        Dim dimension1 As Single() = {0, 0, 1, 1}
        Dim dimension2 As Single() = {0, 1, 1, 0}
        For i = 0 To 3
            data.Add(dimension1(i) + start.x)
            data.Add(start.y)
            data.Add(dimension2(i) + start.z)
            data.Add(1)
        Next
    End Sub

    Private Shared Sub XPlane(ByVal start As COORD3, ByRef data As List(Of Single))
        Dim dimension1 As Single() = {0, 0, 1, 1}
        Dim dimension2 As Single() = {0, 1, 1, 0}
        For i = 0 To 3
            data.Add(start.x)
            data.Add(dimension2(i) + start.y)
            data.Add(dimension1(i) + start.z)
            data.Add(1)
        Next
    End Sub

    Private Shared Sub ZPlane(ByVal start As COORD3, ByRef data As List(Of Single))
        Dim dimension1 As Single() = {0, 0, 1, 1}
        Dim dimension2 As Single() = {0, 1, 1, 0}
        For i = 0 To 3
            data.Add(dimension1(i) + start.x)
            data.Add(dimension2(i) + start.y)
            data.Add(start.z)
            data.Add(1)
        Next
    End Sub

    Public Shared Function ChunkToAbsoluteWorldCoord(position As Integer, chunkPos As RenderWorld.ChunkCoord) As COORD3
        Dim newCoord As New COORD3
        newCoord.z = ((position And &HF000) >> 12) + chunkPos.z * 16
        newCoord.y = (position And &HFF0) >> 4
        newCoord.x = position Mod 16 + chunkPos.x * 16
        Return newCoord
    End Function

    Public Structure COORD3
        Public x As Single
        Public y As Single
        Public z As Single
    End Structure

    Public Enum TorchBlockAttached
        down = 0
        left = 1
        right = 2
        forward = 3
        backward = 4
    End Enum
End Class
Public Class CraftingClass
    Public Shared Function Craft(Crafting(,) As Integer, itemData As ImportedData.ItemData()) As Inventory.InventoryItem
        Dim crafted As String = "", lines() As String = IO.File.ReadAllLines("Resource\Crafting.txt")
        Dim craftedReverse As String = ""
        Dim counter As Integer = 0
        Dim itemCrafted As New Inventory.InventoryItem
        Dim shift As String
        For i = 0 To 2
            crafted &= "x" & Crafting(0, i) & "x" & Crafting(1, i) & "x" & Crafting(2, i)
            craftedReverse &= "x" & Crafting(2, i) & "x" & Crafting(1, i) & "x" & Crafting(0, i)
        Next
        While crafted.Split("x"c)(1) = "0" And counter < 16
            shift = ""
            For j = 0 To 7
                shift &= "x" & crafted.Split("x"c)(j + 2)
            Next
            crafted = shift & "x0"
            counter += 1
        End While
        counter = 0
        While craftedReverse.Split("x"c)(1) = "0" And counter < 16
            shift = ""
            For j = 0 To 7
                shift &= "x" & craftedReverse.Split("x"c)(j + 2)
            Next
            craftedReverse = shift & "x0"
            counter += 1
        End While
        For i = 0 To lines.Length - 1
            If crafted = lines(i).Split(","c)(2) Or craftedReverse = lines(i).Split(","c)(2) Then
                itemCrafted.itemID = CByte(lines(i).Split(","c)(0))
                itemCrafted.numberOfItems = CByte(lines(i).Split(","c)(3))
                If itemCrafted.itemID >= 100 Then
                    itemCrafted.durability = itemData(itemCrafted.itemID - 100).Durability
                End If
                Return itemCrafted
            End If
        Next
        Return itemCrafted
    End Function
End Class

Public Class Bitmap
    Public width As Int32
    Public height As Int32
    Public data As Colour(,)
    Public texture As IntPtr

    Private c As Integer = 0

    Public Sub ReadData(path As String, water As Boolean, Optional editing As Boolean = False)
        Dim rawData As Byte() = IO.File.ReadAllBytes(path & ".bmp")
        width = ReadInt32(rawData, &H12)
        height = ReadInt32(rawData, &H16)
        ReDim data(width - 1, height - 1)
        For y = 0 To height - 1
            For x = 0 To width - 1
                data(x, y).b = rawData((y * width + x) * 3 + 54)
                data(x, y).g = rawData((y * width + x) * 3 + 55)
                data(x, y).r = rawData((y * width + x) * 3 + 56)
            Next
        Next
        texture = Marshal.AllocHGlobal(width * height * 4)
        For i = 0 To width * height - 1
            Marshal.WriteByte(texture + i * 4, rawData(i * 3 + 56))
            Marshal.WriteByte(texture + i * 4 + 1, rawData(i * 3 + 55))
            Marshal.WriteByte(texture + i * 4 + 2, rawData(i * 3 + 54))
            If Not water Then
                If rawData(i * 3 + 56) = 75 And rawData(i * 3 + 55) = 0 And rawData(i * 3 + 54) = 0 Then 'a red value of 75 means the texture is transparent
                    Marshal.WriteByte(texture + i * 4 + 3, 0)
                Else
                    Marshal.WriteByte(texture + i * 4 + 3, 255)
                End If
            Else
                Marshal.WriteByte(texture + i * 4 + 3, 150)
            End If
        Next
        If editing Then
            For y = 0 To height - 1
                For x = 0 To width - 1
                    data(x, y).g = 0
                    data(x, y).r = 0
                Next
            Next
            WriteData("Output2")
        End If
    End Sub

    Public Sub Resize()
        ReDim Preserve data(width - 1, height - 1)
    End Sub

    Public Sub CreateFromArray(dataIn As Byte(,), colour As Byte)
        For i = 0 To width - 1
            For j = 0 To height - 1
                Select Case colour
                    Case 0
                        data(i, j).r = dataIn(i, j)
                    Case 1
                        data(i, j).g = dataIn(i, j)
                    Case 2
                        data(i, j).b = dataIn(i, j)
                End Select
            Next
        Next
    End Sub

    Public Sub CreateFromArray(dataIn As Byte(), colour As Byte)
        Dim dataIn2D(width - 1, height - 1) As Byte
        For i = 0 To width - 1
            For j = 0 To height - 1
                dataIn2D(i, j) = dataIn(i * 4096 + j)
            Next
        Next
        CreateFromArray(dataIn2D, colour)
    End Sub

    Public Sub WriteData(path As String)
        Dim rawData(width * height * 3 + 53) As Byte
        rawData(0) = Asc("B")
        rawData(1) = Asc("M")
        WriteInt32(rawData, 2, rawData.Length)
        WriteInt32(rawData, &HA, 54)
        WriteInt32(rawData, &HE, 40)
        WriteInt32(rawData, &H12, width)
        WriteInt32(rawData, &H16, height)
        rawData(&H1C) = 24
        For y = 0 To height - 1
            For x = 0 To width - 1
                rawData(3 * (y * width + x) + 54) = data(x, y).b
                rawData(3 * (y * width + x) + 55) = data(x, y).g
                rawData(3 * (y * width + x) + 56) = data(x, y).r
            Next
        Next
        IO.File.WriteAllBytes(path & ".bmp", rawData)
    End Sub

    Private Function ReadInt32(ByRef data As Byte(), offset As Int32) As Int32
        Dim value As Int32 = 0
        For i = 3 To 0 Step -1
            value *= 256
            value += data(offset + i)
        Next
        Return value
    End Function

    Private Sub WriteInt32(ByRef data As Byte(), ByVal offset As Int32, ByVal value As Int32)
        For i = 0 To 3
            data(i + offset) = CByte(value Mod 256)
            value \= 256
        Next
    End Sub

    Public Structure Colour
        Dim r As Byte
        Dim g As Byte
        Dim b As Byte
    End Structure
End Class

Public Class MainMenu
    Public Shared Sub MainMenu(ByRef loadGame As Boolean, ByRef saveName As String)
        Dim selection As Integer = 0
        If Not IO.Directory.Exists("SavedGames") Then
            IO.Directory.CreateDirectory("SavedGames")
        End If
        Do
            selection = OptionSelect({"PLAY GAME", "EXIT GAME"})
            If selection = 0 Then
                selection = OptionSelect({"NEW GAME", "LOAD GAME", "BACK"})
                If selection = 0 Then
                    loadGame = False
                    saveName = GetSaveName()
                    Exit Do
                ElseIf selection = 1 Then
                    loadGame = True
                    saveName = GetGameSave()
                    If saveName <> "Back" Then
                        Exit Do
                    End If
                Else
                    Continue Do
                End If
            ElseIf selection = 1 Then
                IO.Directory.Delete("Resource", True)
                MouseInput.ShowMouseCursor()
                End
            End If
        Loop Until False
    End Sub

    Private Shared Function GetSaveName() As String
        Dim name As String
        Clear()
        Title()
        WriteLine("Enter a unique game save name using only alphanumeric characters up to 20 charcters long:")
        Do
            name = ReadLine()
            If IsDuplicateName(name) Then
                WriteLine("There is already a save with this name.")
            ElseIf Not IsValidName(name) Then
                WriteLine("This is an invalid name.")
            Else
                Return name
            End If
        Loop
    End Function

    Private Shared Function IsValidName(toCheck As String) As Boolean
        Dim validCharacters As String = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNMM1234567890"
        If toCheck.Length > 20 Then Return False
        If toCheck.Length = 0 Then Return False
        For i = 0 To toCheck.Length - 1
            If Not validCharacters.Contains(toCheck(i)) Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Shared Function IsDuplicateName(toCheck As String) As Boolean
        Dim savedGames As String() = IO.Directory.GetFiles("SavedGames")
        For i = 0 To savedGames.Length - 1
            If toCheck.ToUpper = savedGames(i).Split("\"c)(1).Split("."c)(0).ToUpper Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Shared Function GetGameSave() As String
        Dim selection As Integer = 0
        Dim key As Integer
        Dim savedGames As String() = IO.Directory.GetFiles("SavedGames")
        Clear()
        Title()
        For i = 0 To savedGames.Length - 1
            WriteLine("".PadRight(15, " "c) & savedGames(i).Split("\"c)(1).Split("."c)(0))
        Next
        WriteLine("".PadRight(15, " "c) & "BACK")
        SetCursorPosition(5, 10 + selection)
        Write("---->>")
        Do
            CursorVisible = False
            If KeyAvailable Then
                key = ReadKey().Key
                SetCursorPosition(5, 10 + selection)
                Write("      ")
                Select Case key
                    Case 38
                        selection -= 1
                    Case 40
                        selection += 1
                End Select
                selection += savedGames.Length + 1
                selection = selection Mod (savedGames.Length + 1)
                SetCursorPosition(5, 10 + selection)
                Write("---->>")
            End If
        Loop Until key = 13
        If selection = savedGames.Length Then Return "Back" Else Return savedGames(selection).Split("\"c)(1).Split("."c)(0)
    End Function

    Private Shared Function OptionSelect(prompt As String()) As Integer
        Dim key As ConsoleKey
        Dim selection As Integer
        CursorVisible = False
        Clear()
        SetCursorPosition(0, 0)
        Title()
        Menu(prompt)
        selection = Math.Abs(selection) Mod prompt.Length
        SetCursorPosition(5, 11 + 3 * selection) : Write("---->>")
        Do
            CursorVisible = False
            If KeyAvailable Then
                key = ReadKey.Key()
                If key = 38 Then selection -= 1
                If key = 40 Then selection += 1
                SetCursorPosition(0, 0)
                Title()
                Menu(prompt)
                selection = Math.Abs(selection) Mod prompt.Length
                SetCursorPosition(5, 11 + 3 * selection) : Write("---->>")
            End If
        Loop Until key = 13
        Return selection
    End Function

    Public Shared Sub Instructions()
        Clear()
        Title()
        WriteLine(IO.File.ReadAllText("Resource\Instructions.txt"))
        ReadLine()
    End Sub

    Private Shared Sub Title()
        WriteLine("  __  __ _____ _   _ ______ _____ _____            ______ _______ ")
        WriteLine(" |  \/  |_   _| \ | |  ____/ ____|  __ \     /\   |  ____|__   __|")
        WriteLine(" | \  / | | | |  \| | |__ | |    | |__) |   /  \  | |__     | |   ")
        WriteLine(" | |\/| | | | | . ` |  __|| |    |  _  /   / /\ \ |  __|    | |   ")
        WriteLine(" | |  | |_| |_| |\  | |___| |____| | \ \  / ____ \| |       | |   ")
        WriteLine(" |_|  |_|_____|_| \_|______\_____|_|  \_\/_/    \_\_|       |_|   ")
        WriteLine()
        WriteLine()
        WriteLine()
        WriteLine()
    End Sub

    Private Shared Sub Menu(prompt As String())
        For i = 0 To prompt.Length - 1
            WriteLine("               /---------------------------------\")
            WriteLine("               |" & prompt(i).PadLeft(17 + prompt(i).Length \ 2, " "c).PadRight(33, " "c) & "|")
            WriteLine("               \---------------------------------/")
        Next
    End Sub
End Class

Public Class RayTracing
    Public Shared Function Trace(playerAngle As Single, playerElevation As Single, playerX As Single, playerY As Single, playerZ As Single, rayRange As Single) As Intersection
        Dim key As KeyboardInput.Keys = KeyboardInput.GetKeys
        Dim nearest As Integer = -1
        Dim nearest2 As Integer = -1
        If key.shift Then
            key = key
        End If
        Dim intersectX As Intersection = XPlaneTrace(playerAngle, playerElevation, playerX, playerY, playerZ, rayRange)
        Dim intersectY As Intersection = YPlaneTrace(playerAngle, playerElevation, playerX, playerY, playerZ, rayRange)
        Dim intersectZ As Intersection = ZPlaneTrace(playerAngle, playerElevation, playerX, playerY, playerZ, rayRange)
        Dim distX As Single = GetDistance(intersectX, playerX, playerY, playerZ)
        Dim distY As Single = GetDistance(intersectY, playerX, playerY, playerZ)
        Dim distZ As Single = GetDistance(intersectZ, playerX, playerY, playerZ)

        If distX > rayRange Then intersectX.failed = True
        If distY > rayRange Then intersectY.failed = True
        If distZ > rayRange Then intersectZ.failed = True

        If distX > distZ Then
            If distY > distZ Then
                nearest = 2
            Else
                nearest = 1
            End If
        ElseIf distY > distX Then
            nearest = 0
        Else
            nearest = 1
        End If

        distX = GetDistanceSng(intersectX, playerX, playerY, playerZ)
        distY = GetDistanceSng(intersectY, playerX, playerY, playerZ)
        distZ = GetDistanceSng(intersectZ, playerX, playerY, playerZ)

        If distX > rayRange Then intersectX.failed = True
        If distY > rayRange Then intersectY.failed = True
        If distZ > rayRange Then intersectZ.failed = True

        If distX > distZ Then
            If distY > distZ Then
                nearest2 = 2
            Else
                nearest2 = 1
            End If
        ElseIf distY > distX Then
            nearest2 = 0
        Else
            nearest2 = 1
        End If

        Select Case nearest2
            Case 0
                Return intersectX
            Case 1
                Return intersectY
            Case 2
                Return intersectZ
        End Select
    End Function

    Public Shared Function GetDistance(intersect As Intersection, playerX As Single, playerY As Single, playerZ As Single) As Single
        Dim x, y, z As Single
        z = CSng((playerZ - intersect.z) ^ 2)
        x = CSng((playerX - intersect.x) ^ 2)
        y = CSng((playerY - intersect.y) ^ 2)

        If intersect.plane = Faces.forward Or intersect.plane = Faces.backwards Then
            'z = Math.Abs(playerZ - intersect)
        End If
        Return x + y + z
    End Function

    Public Shared Function GetDistanceSng(intersect As Intersection, playerX As Single, playerY As Single, playerZ As Single) As Single
        Dim x, y, z As Single
        z = CSng((playerZ - intersect.zSng) ^ 2)
        x = CSng((playerX - intersect.xSng) ^ 2)
        y = CSng((playerY - intersect.ySng) ^ 2)

        If intersect.plane = Faces.forward Or intersect.plane = Faces.backwards Then
            'z = Math.Abs(playerZ - intersect)
        End If
        Return x + y + z
    End Function


    Public Shared Function ZPlaneTrace(playerAngle As Single, playerElevation As Single, playerX As Single, playerY As Single, playerZ As Single, rayRange As Single) As Intersection
        Dim intersect As New Intersection
        Dim x1, y1, z1 As Single
        Dim dX, dY As Single
        Dim fractionalDistance As Single
        Dim offsetRequired As Integer
        Dim backwards As Boolean
        While playerAngle < 0
            playerAngle += CSng(Math.PI * 2)
        End While
        backwards = Math.Floor(playerAngle / Math.PI + Math.PI / 4) Mod 2 = 1

        dX = CSng(Math.Tan(playerAngle))
        dY = CSng(Math.Tan(playerElevation) / Math.Cos(playerAngle))

        If Not (IsNumeric(dY)) Or Not (IsNumeric(dX)) Then
            intersect.failed = True
            Return intersect
        End If

        fractionalDistance = CSng(Math.Ceiling(playerZ) - playerZ)
        If backwards Then fractionalDistance = CSng(Math.Floor(playerZ) - playerZ - 0.01)

        x1 = playerX + dX * fractionalDistance
        y1 = playerY + dY * fractionalDistance
        z1 = CSng(Math.Ceiling(playerZ))
        If backwards Then z1 = CSng(Math.Floor(playerZ) - 0.01)

        offsetRequired = OffsetTrace(New Coord3(dX, dY, 1), New Coord3(x1, y1, z1))
        If backwards Then offsetRequired = OffsetTrace(New Coord3(-1 * dX, dY * -1, -1), New Coord3(x1, y1, z1))

        If offsetRequired = -1 Then
            intersect.failed = True
            Return intersect
        End If

        If backwards Then offsetRequired *= -1

        x1 += offsetRequired * dX
        y1 += offsetRequired * dY
        z1 += offsetRequired

        intersect.plane = Faces.forward
        If backwards Then intersect.plane = Faces.backwards

        intersect.xSng = x1
        intersect.ySng = y1
        intersect.zSng = z1

        intersect.x = CInt(Math.Floor(x1))
        intersect.y = CInt(Math.Floor(y1))
        intersect.z = CInt(Math.Floor(z1))
        Return intersect
    End Function

    Public Shared Function XPlaneTrace(playerAngle As Single, playerElevation As Single, playerX As Single, playerY As Single, playerZ As Single, rayRange As Single) As Intersection
        Dim intersect As New Intersection
        Dim x1, y1, z1 As Single
        Dim dZ, dY As Single
        Dim fractionalDistance As Single
        Dim offsetRequired As Integer
        Dim backwards As Boolean
        While playerAngle < 0
            playerAngle += CSng(Math.PI * 2)
        End While
        backwards = Math.Floor(playerAngle / Math.PI) Mod 2 = 1

        dZ = CSng(1 / Math.Tan(playerAngle))
        dY = CSng(Math.Tan(playerElevation) / Math.Sin(playerAngle))

        If Not IsNumeric(dY) Or Not IsNumeric(dZ) Or Math.Abs(Math.Sin(playerAngle)) < 0.01 Then
            intersect.failed = True
            Return intersect
        End If

        fractionalDistance = CSng(Math.Ceiling(playerX) - playerX)
        If backwards Then fractionalDistance = CSng(Math.Floor(playerX) - playerX - 0.01)

        z1 = playerZ + dZ * fractionalDistance
        y1 = playerY + dY * fractionalDistance
        x1 = CSng(Math.Ceiling(playerX))
        If backwards Then x1 = CSng(Math.Floor(playerX) - 0.01)

        offsetRequired = OffsetTrace(New Coord3(1, dY, dZ), New Coord3(x1, y1, z1))
        If backwards Then offsetRequired = OffsetTrace(New Coord3(-1, dY * -1, dZ * -1), New Coord3(x1, y1, z1))

        If offsetRequired = -1 Then
            intersect.failed = True
            Return intersect
        End If

        If backwards Then offsetRequired *= -1

        x1 += offsetRequired
        y1 += offsetRequired * dY
        z1 += offsetRequired * dZ

        intersect.plane = Faces.right
        If backwards Then intersect.plane = Faces.left

        intersect.xSng = x1
        intersect.ySng = y1
        intersect.zSng = z1

        intersect.x = CInt(Math.Floor(x1))
        intersect.y = CInt(Math.Floor(y1))
        intersect.z = CInt(Math.Floor(z1))
        Return intersect
    End Function

    Public Shared Function YPlaneTrace(playerAngle As Single, playerElevation As Single, playerX As Single, playerY As Single, playerZ As Single, rayRange As Single) As Intersection
        Dim intersect As New Intersection
        Dim x1, y1, z1 As Single
        Dim dZ, dX As Single
        Dim fractionalDistance As Single
        Dim offsetRequired As Integer
        Dim backwards As Boolean
        While playerElevation < 0
            playerElevation += CSng(Math.PI * 2)
        End While
        backwards = Math.Floor(playerElevation / Math.PI) Mod 2 = 1

        dZ = CSng(Math.Cos(playerAngle) / Math.Tan(playerElevation))
        dX = CSng(Math.Sin(playerAngle) / Math.Tan(playerElevation))

        If Not (IsNumeric(dX)) Or Not (IsNumeric(dZ)) Or Math.Abs(Math.Tan(playerAngle)) < 0.01 Then
            intersect.failed = True
            Return intersect
        End If

        fractionalDistance = CSng(Math.Ceiling(playerY) - playerY)
        If backwards Then fractionalDistance = CSng(Math.Floor(playerY) - playerY - 0.01)

        z1 = playerZ + dZ * fractionalDistance
        x1 = playerX + dX * fractionalDistance
        y1 = CSng(Math.Ceiling(playerY))
        If backwards Then y1 = CSng(Math.Floor(playerY) - 0.01)

        offsetRequired = OffsetTrace(New Coord3(dX, 1, dZ), New Coord3(x1, y1, z1))
        If backwards Then offsetRequired = OffsetTrace(New Coord3(dX * -1, -1, dZ * -1), New Coord3(x1, y1, z1))

        If offsetRequired = -1 Then
            intersect.failed = True
            Return intersect
        End If

        If backwards Then offsetRequired *= -1

        x1 += offsetRequired * dX
        y1 += offsetRequired
        z1 += offsetRequired * dZ

        intersect.plane = Faces.down
        If backwards Then intersect.plane = Faces.up

        intersect.xSng = x1
        intersect.ySng = y1
        intersect.zSng = z1

        intersect.x = CInt(Math.Floor(x1))
        intersect.y = CInt(Math.Floor(y1))
        intersect.z = CInt(Math.Floor(z1))
        Return intersect
    End Function

    Public Shared Function PointingAtCuboid(playerAngle As Single, playerElevation As Single, playerX As Single, playerY As Single, playerZ As Single, targetBaseX As Single, targetBaseY As Single, targetBaseZ As Single, targetOrientation As Single, targetDX As Single, targetDY As Single, targetDZ As Single) As Boolean
        Dim coords As Coord3() = GetCuboidCoords(targetBaseX, targetBaseY, targetBaseZ, targetDX, targetDY, targetDZ, targetOrientation)
        Dim relativeCoords(coords.Length - 1) As Coord2
        Dim valid(7) As Boolean
        Dim right As Boolean = False
        Dim left As Boolean = False
        Dim intersectX As Single
        Dim x1, x2, y1, y2 As Single
        For i = 0 To 7
            relativeCoords(i) = RayAngle(coords(i), playerX, playerY, playerZ)
        Next
        For i = 0 To relativeCoords.Length - 1
            relativeCoords(i).x -= playerAngle
            If relativeCoords(i).x < 0 Then
                relativeCoords(i).x += CSng(Math.PI * 2)
            End If
            relativeCoords(i).y -= playerElevation
            If relativeCoords(i).x > Math.PI / 2 And relativeCoords(i).x < Math.PI * 3 / 2 Then
                valid(i) = False
            Else
                valid(i) = True
            End If
            If relativeCoords(i).x > Math.PI Then
                relativeCoords(i).x -= CSng(2 * Math.PI)
            End If
        Next
        For i = 0 To relativeCoords.Length - 1
            For j = 0 To relativeCoords.Length - 1
                If relativeCoords(i).y > relativeCoords(j).y Then
                    If relativeCoords(i).y > 0 And relativeCoords(j).y < 0 Then
                        x1 = relativeCoords(j).x
                        x2 = relativeCoords(i).x
                        y1 = relativeCoords(j).y
                        y2 = relativeCoords(i).y
                        intersectX = (x1 - x2) / (y2 - y1) * y1 + x1
                        If intersectX < 0 Then left = True
                        If intersectX > 0 Then right = True
                        If right And left Then Return True
                    End If
                End If
            Next
        Next
        Return False
    End Function

    Private Shared Function RayAngle(input As Coord3, playerX As Single, playerY As Single, playerZ As Single) As Coord2
        Dim difference As New Coord3(playerX - input.x, input.y - playerY, playerZ - input.z)
        Dim orientation As Single = CSng(Math.Atan(difference.x / difference.z))
        Dim elevation As Single = CSng(Math.Atan(difference.y / (difference.x * difference.x + difference.z * difference.z) ^ 0.5F))
        If difference.z > 0 Then
            orientation += CSng(Math.PI)
        End If
        If orientation < 0 Then
            orientation += CSng(Math.PI * 2)
        End If
        If orientation > Math.PI * 2 Then
            orientation -= CSng(Math.PI * 2)
        End If
        Return New Coord2(orientation, elevation)
    End Function

    Private Shared Function GetCuboidCoords(baseX As Single, baseY As Single, baseZ As Single, dx As Single, dy As Single, dz As Single, orientation As Single) As Coord3()
        Dim xChange As Single() = {0, 0, 0, 0, 1, 1, 1, 1}
        Dim yChange As Single() = {0, 0, 1, 1, 0, 0, 1, 1}
        Dim zChange As Single() = {0, 1, 0, 1, 0, 1, 0, 1}
        Dim sin As Single = CSng(Math.Sin(orientation))
        Dim cos As Single = CSng(Math.Cos(orientation))
        Dim coords(7) As Coord3
        For i = 0 To xChange.Length - 1
            xChange(i) -= 0.5F
            zChange(i) -= 0.5F

            xChange(i) *= dx
            yChange(i) *= dy
            zChange(i) *= dz
        Next
        For i = 0 To 7
            coords(i) = New Coord3(baseX + xChange(i) * cos - zChange(i) * sin, baseY + yChange(i), baseZ + zChange(i) * cos + xChange(i) * sin)
        Next
        Return coords
    End Function

    Public Shared Function OffsetTrace(delta As Coord3, start As Coord3) As Integer
        Dim numTraces As Integer = 0
        Dim distanceSquared As Single
        Dim targetBlock As Byte
        If delta.x < 0 Then
            delta.x = delta.x
        End If
        distanceSquared = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z
        Do
            If Math.Floor(delta.y * numTraces + start.y) < 0 Or Math.Floor(delta.y * numTraces + start.y) > 255 Then Return -1
            Try
                targetBlock = RenderWorld.GetBlock(CInt(Math.Floor(delta.x * numTraces + start.x)), CInt(Math.Floor(delta.y * numTraces + start.y)), CInt(Math.Floor(delta.z * numTraces + start.z)))
                If targetBlock <> Terrain.Blocks.air And targetBlock <> Terrain.Blocks.water Then
                    Return numTraces
                End If
            Catch
                Return -1
            End Try
            If numTraces * distanceSquared > 10 Then Return -1
            numTraces += 1
        Loop
    End Function

    Public Structure Coord3
        Public x As Single
        Public y As Single
        Public z As Single
        Sub New(xIn As Single, yIn As Single, zIn As Single)
            x = xIn
            y = yIn
            z = zIn
        End Sub
    End Structure

    Public Structure Coord2
        Public x As Single
        Public y As Single
        Sub New(xIn As Single, yIn As Single)
            x = xIn
            y = yIn
        End Sub
    End Structure

    Public Structure Intersection
        Dim failed As Boolean
        Dim x As Integer
        Dim y As Integer
        Dim z As Integer
        Dim xSng As Single
        Dim ySng As Single
        Dim zSng As Single
        Dim plane As Faces
        Sub New(copy As Intersection)
            x = copy.x
            y = copy.y
            z = copy.z
            plane = copy.plane
            failed = copy.failed
        End Sub
    End Structure

    Public Enum Faces
        forward = 0
        backwards = 1
        left = 2
        right = 3
        up = 4
        down = 5
    End Enum
End Class

Public Class Zombie
    Public Const DESPAWNDISTANCE As Integer = 10000
    Public Const SPEED As Single = 1.5F
    Public Const ROTATESPEED As Single = 1
    Public Const COOLDOWN As Single = 2
    Public Const ATTACKDAMAGE As Integer = 3

    Public baseXCentre As Single
    Public baseYCentre As Single
    Public baseZCentre As Single
    Public velY As Single
    Public orientation As Single
    Public inUse As Boolean
    Public lastAttack As Single = 0
    Public red As Boolean = False
    Public green As Boolean = False
    Public health As Integer
    Public redTime As Single
    Private timeGrowl As Single = 0

    Sub New(Init As Boolean)
        inUse = False
    End Sub

    Public Function IsValidSpawn(x As Single, y As Single, z As Single, daylight As Single) As Boolean
        Dim isValid As Boolean = True
        Dim floorX As Integer = CInt(Math.Floor(x))
        Dim floorY As Integer = CInt(Math.Floor(y))
        Dim floorZ As Integer = CInt(Math.Floor(z))
        Dim lightN, lightA As Byte

        If y < 3 Then Return False

        Dim baseBlock As Byte = RenderWorld.GetBlock(floorX, floorY - 1, floorZ)
        If baseBlock = Terrain.Blocks.air OrElse baseBlock = Terrain.Blocks.torch Then Return False
        For i = 0 To 1
            If RenderWorld.GetBlock(floorX, floorY + i, floorZ) <> Terrain.Blocks.air Then Return False
        Next
        RenderWorld.GetLightBlock(floorX, floorY, floorZ, lightA, lightN)
        If GetDistanceToPlayerSquared(Player.x + Player.chunkX * 16, Player.y, Player.chunkZ * 16 + Player.z, x, y, z) < 100 Then Return False
        Return lightA <= 4 And lightN * lightN * daylight <= 16
    End Function

    Sub Initialise(x As Single, y As Single, z As Single)
        inUse = True
        baseXCentre = x
        baseYCentre = y
        baseZCentre = z
        health = 20
    End Sub
    Public Function ShouldDespawn(tX As Single, tY As Single, tZ As Single) As Boolean
        Return DistanceFromTarget(tX, baseYCentre, tZ) > DESPAWNDISTANCE
    End Function
    Public Function DesiredOrientation(targetX As Single, targetY As Single, targetZ As Single) As Single
        Dim orientation As Single = CSng(Math.Atan((targetX - baseXCentre) / (targetZ - baseZCentre)))
        If targetZ < baseZCentre Then
            orientation += CSng(Math.PI)
        End If
        If orientation < 0 Then
            orientation += CSng(Math.PI * 2)
        End If
        If orientation > Math.PI * 2 Then
            orientation -= CSng(Math.PI * 2)
        End If
        Return orientation
    End Function
    Private Function DistanceFromTarget(targetX As Single, targetY As Single, targetZ As Single) As Single
        Dim distance As Single = 0
        distance += CSng((targetX - baseXCentre) ^ 2)
        distance += CSng((targetY - baseYCentre) ^ 2)
        distance += CSng((targetZ - baseZCentre) ^ 2)
        Return distance
    End Function
    Public Function HurtPlayer(targetX As Single, targetY As Single, targetZ As Single) As Boolean
        Return DistanceFromTarget(targetX, baseYCentre, targetZ) < 0.6F And Math.Abs(baseYCentre - targetY) < 1.9F
    End Function
    Public Function ShouldRotateClockwise(start As Single, finish As Single) As Boolean
        If finish > start Then
            start += CSng(Math.PI * 2)
        End If
        Return start - finish > Math.PI
    End Function

    Public count As Integer = 0

    Public Sub Growl(ByRef soundData As Sound.SoundData)
        If Timer - timeGrowl > 5.0F Then
            If Rnd() > 0.9F Then
                Sound.PlayAdditionalSound("zombie", soundData)
                timeGrowl = CSng(Timer)
            End If
        End If
    End Sub

    Private Function GetDistanceToPlayerSquared(px As Single, py As Single, pz As Single, x As Single, y As Single, z As Single) As Single
        Return (px - x) * (px - x) + (py - y) * (py - y) + (pz - z) * (pz - z)
    End Function

    Public Sub Move(deltaTime As Single, targetX As Single, targetY As Single, targetZ As Single, ByRef soundData As Sound.SoundData)
        Dim targetOrientation As Single = DesiredOrientation(targetX, targetY, targetZ)
        Dim oldX, oldY, oldZ As Single
        Dim clockwise As Boolean = ShouldRotateClockwise(orientation, targetOrientation)
        Dim moveRight As Boolean

        If GetDistanceToPlayerSquared(targetX, targetY, targetZ, baseXCentre, baseYCentre, baseZCentre) < 1000 Then
            Growl(soundData)
        End If

        Gravity(deltaTime)
        redTime -= deltaTime

        red = redTime > 0

        oldX = baseXCentre
        oldY = baseYCentre
        oldZ = baseZCentre

        If clockwise Then
            orientation += deltaTime * ROTATESPEED
        Else
            orientation -= deltaTime * ROTATESPEED
        End If
        If clockwise <> ShouldRotateClockwise(orientation, targetOrientation) Then ' If it has gone too far
            orientation = targetOrientation
        End If

        If orientation < 0 Then orientation += CSng(Math.PI * 2)
        If orientation > Math.PI * 2 Then orientation -= CSng(Math.PI * 2)

        moveRight = targetX > baseXCentre
        If Math.Abs(orientation - targetOrientation) < 1 Then
            baseXCentre += CSng(Math.Sin(orientation)) * SPEED * deltaTime
            baseZCentre += CSng(Math.Cos(orientation)) * SPEED * deltaTime
            If DistanceFromTarget(targetX, baseYCentre, targetZ) < 0.5F And Math.Abs(baseYCentre - targetY) < 1.8F Then
                baseXCentre = oldX
                baseYCentre = oldY
                baseZCentre = oldZ
            End If
        End If

        DeStick(oldX, oldY, oldZ)

        If Not IsValidPosition(baseXCentre, baseYCentre, baseZCentre) Then
            If IsValidPosition(baseXCentre, baseYCentre + 1, baseZCentre) And baseYCentre - Math.Floor(baseYCentre) < 0.3F Then
                velY = 5.5F
            End If
            baseXCentre = oldX
            baseYCentre = oldY
            baseZCentre = oldZ
        End If

        If ShouldDespawn(targetX, targetY, targetZ) Then Despawn()
    End Sub

    Private Sub DeStick(ByRef x As Single, ByRef y As Single, ByRef z As Single)
        If Not IsValidPosition(x, y, z) Then
            If x - Math.Floor(x) < 0.2F Then
                x = CSng(Math.Floor(x) + 0.2F)
            End If
            If z - Math.Floor(x) < 0.2F Then
                z = CSng(Math.Floor(z) + 0.2F)
            End If
            If x - Math.Floor(x) > 0.8F Then
                x = CSng(Math.Floor(x) + 0.8F)
            End If
            If z - Math.Floor(x) > 0.8F Then
                z = CSng(Math.Floor(z) + 0.8F)
            End If
        End If
    End Sub

    Private Function IsValidPosition(x As Single, y As Single, z As Single) As Boolean
        Const RANGE As Single = 0.2F
        For i = -1 To 1
            For j = -1 To 1
                If Not Player.CanMoveThroughBlock(GetBlock(x + RANGE * i, y, z + RANGE * j)) Then Return False
            Next
        Next
        Return True
    End Function

    Public Sub GenerateFaces(ByRef faces As List(Of Byte), ByRef coords As List(Of Single), ByRef colours As List(Of Byte))
        Dim modelCoords As New List(Of Single)
        modelCoords.AddRange(Cuboid(0, 1.5F, 0, 0.5F, 0.5F, 0.5F))
        modelCoords.AddRange(Cuboid(0, 0.75F, 0, 0.5F, 0.75F, 0.25F))
        modelCoords.AddRange(RotatePitch(Cuboid(0.375F, 0.875F, -0.125F, 0.25F, 0.75F, 0.25F), CSng(Math.PI * 0.5F + Math.Sin(Timer) * 0.1F), 1.5F, 0F))
        modelCoords.AddRange(RotatePitch(Cuboid(-0.375F, 0.875F, -0.125F, 0.25F, 0.75F, 0.25F), CSng(Math.PI * 0.5F + Math.Sin(Timer) * 0.1F), 1.5F, 0F))
        modelCoords.AddRange(RotatePitch(Cuboid(0.125F, 0, 0, 0.25F, 0.75F, 0.25F), CSng(Math.Sin(Timer * Math.PI * 2 * SPEED / 1.5F) * 0.5F), 0.75F, 0F))
        modelCoords.AddRange(RotatePitch(Cuboid(-0.125F, 0, 0, 0.25F, 0.75F, 0.25F), CSng(-Math.Sin(Timer * Math.PI * 2 * SPEED / 1.5F) * 0.5F), 0.75F, 0F))
        For i = 0 To 35
            faces.Add(CByte(RenderWorld.ZombieTextureStart + i))
        Next
        For i = 1 To 24 * 6
            If Not green Then
                colours.Add(255)
                If red Then
                    colours.Add(0)
                    colours.Add(0)
                Else
                    colours.Add(255)
                    colours.Add(255)
                End If
            Else
                colours.Add(0)
                colours.Add(255)
                colours.Add(0)
            End If
        Next
        coords.AddRange(Translate(RotateModel(modelCoords, orientation), baseXCentre, baseYCentre, baseZCentre))
    End Sub

    Private Function RotatePitch(ByRef coord As List(Of Single), angle As Single, axisY As Single, axisZ As Single) As List(Of Single)
        Dim newCoord As List(Of Single) = Translate(coord, 0, axisY * -1, axisZ * -1)
        Dim rotated As New List(Of Single)
        Dim sin As Single = CSng(Math.Sin(angle))
        Dim cos As Single = CSng(Math.Cos(angle))
        For i = 0 To newCoord.Count - 4 Step 4
            rotated.Add(newCoord(i))
            rotated.Add(newCoord(i + 1) * cos + newCoord(i + 2) * sin)
            rotated.Add(newCoord(i + 2) * cos - newCoord(i + 1) * sin)
            rotated.Add(newCoord(i + 3))
        Next
        Return Translate(rotated, 0, axisY, axisZ)
    End Function

    Private Function GetBlock(x As Single, y As Single, z As Single) As Byte
        Return RenderWorld.GetBlock(CInt(Math.Floor(x)), CInt(Math.Floor(y)), CInt(Math.Floor(z)))
    End Function

    Public Sub AttackPlayer(ByRef PlayerHealth As Integer, playerX As Single, playerY As Single, playerZ As Single)
        If Timer - lastAttack > COOLDOWN AndAlso HurtPlayer(playerX, playerY, playerZ) Then
            lastAttack = CSng(Timer)
            PlayerHealth -= ATTACKDAMAGE
        End If
    End Sub

    Private Sub Gravity(deltaTime As Single)
        Dim deltaY, deltaYCheck As Single
        Dim x, y, z As Single
        Dim baseBlock As Integer
        x = baseXCentre - 0.5F
        y = baseYCentre
        z = baseZCentre - 0.5F
        If Not Player.CanMoveThroughBlock(GetBlock(x, y + 1.9F, z)) Then
            velY *= -0.5F
        End If
        baseBlock = GetBlock(x, y - 0.1F, z)
        If baseBlock = Terrain.Blocks.air Or baseBlock = Terrain.Blocks.torch And IsValidPosition(x, y - 0.2F, z) Then
            velY -= deltaTime * 13
            If velY < -10 Then velY = -10
        ElseIf baseBlock = Terrain.Blocks.water Then
            velY -= deltaTime * 5
            If velY < -5 Then velY = -5
        Else
            If velY < 0.1 Then
                y = CSng(Math.Floor(y) + 0.05)
                velY = 0
            End If
        End If
        deltaY = deltaTime * velY
        If deltaY < 0 Then
            deltaYCheck = 0
            While deltaYCheck > deltaY
                deltaYCheck -= 1
                If deltaYCheck < deltaY Then deltaYCheck = deltaY
                baseBlock = GetBlock(x, y - 0.1F + deltaYCheck, z)
                If Not Player.CanMoveThroughBlock(baseBlock) Then
                    deltaY = deltaYCheck
                End If
            End While
        End If
        y += deltaY
        baseBlock = GetBlock(x, y, z)
        If Not Player.CanMoveThroughBlock(baseBlock) Then
            y = CSng(Math.Ceiling(y) + 0.05)
        End If
        baseYCentre = y
    End Sub

    Private Function RotateModel(ByRef coord As List(Of Single), angle As Single) As List(Of Single)
        Dim rotated As New List(Of Single)
        Dim sin As Single = CSng(Math.Sin(angle))
        Dim cos As Single = CSng(Math.Cos(angle))
        For i = 0 To coord.Count - 4 Step 4
            rotated.Add(coord(i) * cos + coord(i + 2) * sin)
            rotated.Add(coord(i + 1))
            rotated.Add(coord(i + 2) * cos - coord(i) * sin)
            rotated.Add(coord(i + 3))
        Next
        Return rotated
    End Function

    Private Function Translate(ByRef coord As List(Of Single), x As Single, y As Single, z As Single) As List(Of Single)
        Dim newCoords As New List(Of Single)
        For i = 0 To coord.Count - 4 Step 4
            newCoords.Add(coord(i) + x)
            newCoords.Add(coord(i + 1) + y)
            newCoords.Add(coord(i + 2) + z)
            newCoords.Add(coord(i + 3))
        Next
        Return newCoords
    End Function

    Private Function Cuboid(x As Single, y As Single, z As Single, dx As Single, dy As Single, dz As Single) As List(Of Single)
        Dim returnValue As New List(Of Single)
        Dim dimension1 As Single() = {0, 0, 1, 1}
        Dim dimension2 As Single() = {0, 1, 1, 0}
        For j = -1 To 1 Step 2
            For i = 0 To 3
                'returnValue.Add(((dimension1(i) - 0.5F) * dx + x))
                'returnValue.Add(dimension2(i) * dy + y)
                returnValue.Add(x + (j * 0.5F * dx) - dimension1(i) * j)
                returnValue.Add(y + dy + dimension2(i) - 1)
                returnValue.Add(z + dz / 2 * j)
                returnValue.Add(1)
            Next
            For i = 0 To 3
                returnValue.Add(x - dx * 0.5F * j + dimension1(i) * j)
                returnValue.Add(y + dy / 2 * (j + 1))
                returnValue.Add(z + dz * 0.5F + dimension2(i) - 1)
                returnValue.Add(1)
            Next
            For i = 0 To 3
                returnValue.Add(x + dx * 0.5F * j)
                returnValue.Add(y + dy + dimension2(i) - 1)
                returnValue.Add(z - dz * 0.5F * j + dimension1(i) * j)
                returnValue.Add(1)
            Next
        Next
        Return returnValue
    End Function

    Sub GetAttacked(damage As Integer)
        If damage = 0 Then damage = 1
        health -= damage
        redTime = 0.2F
        If health <= 0 Then
            Despawn()
        End If
    End Sub

    Private Sub Despawn()
        inUse = False
    End Sub
End Class

Public Class OpenGL
    Const TEXTURE2D As Int32 = &HDE1
    Const ARRAYVERTEX As Int32 = &H8074
    Const ARRAYTEXTURE As Int32 = &H8078
    Const ARRAYCOLOUR As Int32 = &H8076
    Const MATRIXMODEL As Int32 = &H1700
    Const GL_SRC_APLHA As Int32 = 770
    Const GL_ONE_MINUS_SRC_ALPHA As Int32 = 771
    Const GL_BLEND As Int32 = 3042
    Const GL_DST_COLOR As Int32 = 774

    Structure OpenGlData
        Public names As IntPtr
        'Private Shared verticesPtr As IntPtr
        'Private Shared verticesData(10000 * 4 * 4) As Single
        Public texturesPtr As IntPtr
        Public texturesData As Integer()
        Sub New(init As Boolean)
            ReDim texturesData(10000 * 4 * 2)
        End Sub
        'Private Shared colourPtr As IntPtr
    End Structure

    Public Shared Sub Initialise(image As IntPtr, imageSize As Int32, ByRef OpenGlDataInput As OpenGlData)
        Dim context As IntPtr
        Dim glContext As IntPtr
        Dim desc As PIXELFORMATDESCRIPTOR = NewPixDescriptor()
        Dim format As Int32
        context = GetDC(GetForegroundWindow())
        format = ChoosePixelFormat(context, desc)
        SetPixelFormat(context, format, desc)
        glContext = wglCreateContext(context)
        wglMakeCurrent(context, glContext)
        glEnable(TEXTURE2D)
        glEnable(&HB71)
        glPixelStorei(&HCF5, 1) 'Pixels are row by row
        OpenGlDataInput.names = Marshal.AllocHGlobal(1024)
        glGenTextures(1, OpenGlDataInput.names)
        glBindTexture(TEXTURE2D, Marshal.ReadInt32(OpenGlDataInput.names))
        glTexImage2D(TEXTURE2D, 0, &H1908, 16, imageSize * 16, 0, &H1908, &H1401, image) '&h1401 is byte, &h1404 is int
        OpenGlDataInput.texturesPtr = Marshal.AllocHGlobal(4 * 1000000 * 4 * 4)
        'verticesPtr = Marshal.AllocHGlobal(4 * 1000000 * 4 * 4)
        'colourPtr = Marshal.AllocHGlobal(4 * 1000000 * 4)

        glEnableClientState(ARRAYTEXTURE)
        glEnableClientState(ARRAYVERTEX)
        glEnableClientState(ARRAYCOLOUR)
        'glVertexPointer(4, &H1406, 0, verticesPtr)
        'glColorPointer(3, &H1401, 0, colourPtr)
        glTexCoordPointer(2, &H1406, 0, OpenGlDataInput.texturesPtr)
        glTexParameteri(TEXTURE2D, &H2800, &H2600)
        glTexParameteri(TEXTURE2D, &H2801, &H2600)

    End Sub

    Public Shared Sub RenderGUI(coords As Single(), colours As Byte(), ByRef openglDataInput As OpenGlData)

        glDepthFunc(&H207)
        'Marshal.Copy(coords, 0, verticesPtr, coords.Length)
        glVertexPointer(4, &H1406, 0, coords)
        glColorPointer(3, &H1401, 0, colours)

        glEnable(TEXTURE2D)
        glDisable(ARRAYCOLOUR)
        glTexEnvf(&H2300, &H2200, &H1E01)
        glBindTexture(TEXTURE2D, Marshal.ReadInt32(openglDataInput.names))
        'glTexParameteri(TEXTURE2D, &H2800, &H2600)
        'glTexParameteri(TEXTURE2D, &H2801, &H2600)

        glEnable(&HBC0)
        glAlphaFunc(&H204, 0.5)
        glMatrixMode(MATRIXMODEL)
        glLoadIdentity()
        MatricesScaled()

        'IO.File.AppendAllText("information.txt", coords.Length & vbNewLine)
        Try
            glDrawArrays(7, 0, coords.Length \ 4)
        Catch
        End Try
        glDisable(&HBC0)
    End Sub

    Public Shared Sub MatricesScaled()
        Dim matrixPtr As IntPtr = Marshal.AllocHGlobal(1024)
        Dim size As Window.COORD = Window.GetSize()
        Dim matrixScale As Single() =
    {CSng(size.y / size.x), 0, 0, 0,
    0, 1, 0, 0,
    0, 0, 0.1, 0,
    0, 0, 0, 1
           }
        Marshal.Copy(matrixScale, 0, matrixPtr, matrixScale.Length)
        glMultMatrixf(matrixPtr)

    End Sub


    Public Shared Sub RenderTransparentBlock(imageSize As Integer, coords As Single(), colours As Byte(), ByRef openglDataInput As OpenGlData)
        glDepthFunc(&H203)
        glVertexPointer(4, &H1406, 0, coords)
        'Marshal.Copy(coords, 0, verticesPtr, coords.Length)
        glColorPointer(3, &H1401, 0, colours)
        'Marshal.Copy(colours, 0, colourPtr, colours.Length)

        glEnable(TEXTURE2D)
        glDisable(ARRAYCOLOUR)
        glTexEnvf(&H2300, &H2200, &H1E01)
        glBindTexture(TEXTURE2D, Marshal.ReadInt32(openglDataInput.names))
        'glTexParameteri(TEXTURE2D, &H2800, &H2600)
        'glTexParameteri(TEXTURE2D, &H2801, &H2600)

        glEnable(&HBC0)
        glAlphaFunc(&H204, 0.5)
        glMatrixMode(MATRIXMODEL)
        glLoadIdentity()
        Matrices()

        glDrawArrays(7, 0, CInt(coords.Length / 4))
        glDisable(&HBC0)
    End Sub

    Public Shared Sub UpdateDisplay()
        glFlush()
    End Sub

    Public Shared Sub Clear(r As Single, g As Single, b As Single)
        glClearColor(r, g, b, 1.0)
        glClear(16384 + &H100)
    End Sub

    Public Shared Sub ClearColours(length As Integer)
        Dim col(length - 1) As Byte
        For i = 0 To length - 1
            col(i) = 255
        Next
        glColorPointer(3, &H1401, 0, col)
        'Marshal.Copy(col, 0, colourPtr, length)
    End Sub

    Public Shared Sub RenderScene(playerAngle As Single, playerElevation As Single, verticesData As Single(), numPolys As Integer, colourData As Byte(), ByRef openGlDataInput As OpenGlData)
        glDepthFunc(&H201)
        glEnable(TEXTURE2D)
        glTexEnvf(&H2300, &H2200, &H1E01)
        glBindTexture(TEXTURE2D, Marshal.ReadInt32(openGlDataInput.names))
        'glTexParameteri(TEXTURE2D, &H2800, &H2600)
        'glTexParameteri(TEXTURE2D, &H2801, &H2600)

        Matrices()

        'Marshal.Copy(verticesData, 0, verticesPtr, numPolys * 16)
        glVertexPointer(4, &H1406, 0, verticesData)
        'Marshal.Copy(colourData, 0, colourPtr, numPolys * 16)
        glColorPointer(3, &H1401, 0, colourData)

        glDrawArrays(7, 0, numPolys * 4)
    End Sub

    Public Shared Sub RenderTransluscentBlock(coords As Single(), colours As Byte(), ByRef openGlDataInput As OpenGlData, Optional matrixOverrride As Boolean = False)
        glDepthFunc(&H203)
        'Marshal.Copy(coords, 0, verticesPtr, coords.Length)
        glVertexPointer(4, &H1406, 0, coords)
        'Marshal.Copy(colours, 0, colourPtr, colours.Length)
        glColorPointer(4, &H1401, 0, colours)

        glEnable(TEXTURE2D)
        glEnable(GL_BLEND)
        glDisable(ARRAYCOLOUR)
        glTexEnvf(&H2300, &H2200, &H1E01)
        glBindTexture(TEXTURE2D, Marshal.ReadInt32(openGlDataInput.names))
        'glTexParameteri(TEXTURE2D, &H2800, &H2600)
        'glTexParameteri(TEXTURE2D, &H2801, &H2600)

        glBlendFunc(GL_SRC_APLHA, GL_ONE_MINUS_SRC_ALPHA)
        'glBlendFunc(GL_DST_COLOR, 0)
        glMatrixMode(MATRIXMODEL)
        glLoadIdentity()
        If Not matrixOverrride Then
            Matrices()
        End If

        glDrawArrays(7, 0, CInt(coords.Length / 4))
        glDisable(&HBC0)
        glDisable(GL_BLEND)
    End Sub

    Public Shared Sub InitTextures(faceType As Byte(), imageSize As Single, numFaces As Single, fractions As List(Of FractionalIcon), ByRef OpenGlDataInput As OpenGlData)
        Dim texturesData As Single() = {0, 0, 0, 1, 1, 1, 1, 0}
        Dim fractionIndex As Integer
        Dim baseTexData(texturesData.Length * CInt(imageSize) - 1) As Single
        Dim scaledBaseTexData(texturesData.Length * CInt(imageSize) - 1) As Single
        For i = 0 To CInt(imageSize) - 1
            For j = 0 To texturesData.Length - 1 Step 2
                baseTexData(i * 8 + j) = (texturesData(j))
                baseTexData(i * 8 + j + 1) = ((texturesData(j + 1) + i) / imageSize)
            Next
        Next

        If fractions.Count > 0 Then
            For i = 0 To CInt(numFaces) - 1
                fractionIndex = -1
                For j = 0 To fractions.Count - 1
                    If fractions(j).index = i Then
                        fractionIndex = j
                    End If
                Next
                If fractionIndex = -1 Then
                    Marshal.Copy(baseTexData, faceType(i) * 8, OpenGlDataInput.texturesPtr + i * 32, 8)
                Else
                    'Copy part of a texture
                    For j = 0 To CInt(imageSize) - 1
                        For k = 0 To texturesData.Length - 1 Step 2
                            scaledBaseTexData(j * 8 + k) = (texturesData(k)) * fractions(fractionIndex).x
                            scaledBaseTexData(j * 8 + k + 1) = ((texturesData(k + 1) * fractions(fractionIndex).z + j) / imageSize)
                        Next
                    Next
                    Marshal.Copy(scaledBaseTexData, faceType(i) * 8, OpenGlDataInput.texturesPtr + i * 32, 8)
                End If
            Next
        Else
            For i = 0 To CInt(numFaces) - 1
                Marshal.Copy(baseTexData, faceType(i) * 8, OpenGlDataInput.texturesPtr + i * 32, 8)
            Next
        End If
    End Sub

    Public Structure FractionalIcon
        Public index As Integer
        Public x As Single
        Public z As Single
    End Structure

    Public Shared Sub Matrices()
        glMatrixMode(MATRIXMODEL)
        glLoadIdentity()
        Dim matrixPtr As IntPtr = Marshal.AllocHGlobal(1024)
        Dim matrixPtr2 As IntPtr = Marshal.AllocHGlobal(1024)
        Dim matrixPtr3 As IntPtr = Marshal.AllocHGlobal(1024)
        Dim matrixPtr4 As IntPtr = Marshal.AllocHGlobal(1024)
        Dim matrixPtr5 As IntPtr = Marshal.AllocHGlobal(1024)
        Dim size As Window.COORD = Window.GetSize()
        Dim playerHeight As Single

        If KeyboardInput.GetKeys().shift Then
            playerHeight = 1.6F
        Else
            playerHeight = 1.8F
        End If

        ' MATRIX LAYOUT
        ' a b c d
        ' e f g h
        ' i j k l
        ' m n o p
        '

        ' new x = ax + ey + ... etc

        Dim matrixTrans As Single() =
            {1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            -Player.x - Player.chunkX * 16, -Player.y - playerHeight + CSng(Math.Sin(Player.bounce * 10)) * 0.05F, -Player.z - Player.chunkZ * 16, 1
                   }
        Marshal.Copy(matrixTrans, 0, matrixPtr3, matrixTrans.Length)

        Dim matrixRot As Single() =
            {CSng(Math.Cos(Player.playerAngle)), 0, CSng(Math.Sin(Player.playerAngle)), 0,
            0, 1, 0, 0,
            CSng(-Math.Sin(Player.playerAngle)), 0, CSng(Math.Cos(Player.playerAngle)), 0,
            0, 0, 0, 1
                   }
        Marshal.Copy(matrixRot, 0, matrixPtr, matrixRot.Length)

        Dim matrixElevate As Single() =
            {1, 0, 0, 0,
            0, CSng(Math.Cos(Player.playerElevation)), CSng(Math.Sin(Player.playerElevation)), 0,
            0, CSng(-Math.Sin(Player.playerElevation)), CSng(Math.Cos(Player.playerElevation)), 0,
            0, 0, 0, 1
                   }
        Marshal.Copy(matrixElevate, 0, matrixPtr2, matrixRot.Length)

        Dim matrixPerspective As Single() =
            {1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, -1, 1,
            0, 0, -1, 0
                   }
        Marshal.Copy(matrixPerspective, 0, matrixPtr4, matrixRot.Length)

        Dim matrixScale As Single() =
            {1, 0, 0, 0,
            0, CSng(size.x / size.y), 0, 0,
            0, 0, 0.1, 0,
            0, 0, 0, 1
                   }
        Marshal.Copy(matrixScale, 0, matrixPtr5, matrixRot.Length)

        glMultMatrixf(matrixPtr5)
        glMultMatrixf(matrixPtr4)
        glMultMatrixf(matrixPtr2)
        glMultMatrixf(matrixPtr)
        glMultMatrixf(matrixPtr3)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glBlendFunc(s As Int32, d As Int32)
    End Sub

    <DllImport("Opengl32.dll")>
    Private Shared Sub glRotatef(angle As Single, x As Single, y As Single, z As Single)
    End Sub

    <DllImport("Opengl32.dll")>
    Private Shared Sub glMultMatrixf(matrix As IntPtr)
    End Sub

    <DllImport("Opengl32.dll")>
    Private Shared Sub glDepthFunc(compare As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glDrawArrays(mode As Int32, first As Int32, count As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glEnableClientState(array As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glTexCoordPointer(size As Int32, type As Int32, stride As Int32, ptr As IntPtr)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glColorPointer(size As Int32, type As Int32, stride As Int32, ptr As IntPtr)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glColorPointer(size As Int32, type As Int32, stride As Int32, data As Byte())
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glVertexPointer(size As Int32, type As Int32, stride As Int32, ptr As IntPtr)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glVertexPointer(size As Int32, type As Int32, stride As Int32, data As Single())
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glLoadIdentity()
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glMatrixMode(mode As Int32)
    End Sub

    <DllImport("Glu32.dll")>
    Private Shared Sub gluPerspective(fovy As Double, aspect As Double, near As Double, far As Double)
    End Sub


    <DllImport("OpenGl32.dll")>
    Private Shared Sub glTexImage2D(ByVal target As Int32, ByVal level As Int32, ByVal format As Int32, ByVal width As Int32, ByVal height As Int32, ByVal border As Int32, format2 As Int32, ByVal type As Int32, ByVal pixels As IntPtr)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glTexEnvf(target As Int32, name As Int32, param As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glTexParameteri(target As Int32, name As Int32, param As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glPixelStorei(name As Int32, param As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glEnable(id As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glDisable(id As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glGenTextures(ByVal number As Int32, ByVal names As IntPtr)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glBindTexture(type As Int32, id As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glAlphaFunc(func As Int32, reference As Single)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glRectf(x1 As Single, y1 As Single, x2 As Single, y2 As Single)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glBegin(mode As Int32)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glEnd()
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glTexCoord2f(x As Single, y As Single)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glVertex4f(x As Single, y As Single, z As Single, w As Single)
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glColor3f(r As Single, g As Single, b As Single)
    End Sub

    Private Shared Function NewPixDescriptor() As PIXELFORMATDESCRIPTOR
        Dim pix As New PIXELFORMATDESCRIPTOR
        pix.size = 40
        pix.version = 1
        pix.flags = 36
        pix.pixelType = 0
        pix.colourBits = 32
        pix.aBits = 8
        pix.depth = 24
        pix.pixelType = 0
        Return pix
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Structure PIXELFORMATDESCRIPTOR
        Dim size As Int16
        Dim version As Int16
        Dim flags As Int32
        Dim pixelType As Byte
        Dim colourBits As Byte
        Dim rBits As Byte
        Dim rShift As Byte
        Dim bBits As Byte
        Dim bShift As Byte
        Dim gBits As Byte
        Dim gShift As Byte
        Dim aBits As Byte
        Dim aShift As Byte
        Dim accrBits As Byte
        Dim accgBits As Byte
        Dim accbBits As Byte
        Dim accaBits As Byte
        Dim depth As Byte
        Dim stencil As Byte
        Dim buffer As Byte
        Dim layerType As Byte
        Dim reserved As Byte
        Dim layerMask As Int32
        Dim visibleMask As Int32
        Dim damageMask As Int32
    End Structure

    <DllImport("GDI32.dll")>
    Private Shared Function ChoosePixelFormat(ByVal hnd As IntPtr, ByRef desc As PIXELFORMATDESCRIPTOR) As Int32
    End Function

    <DllImport("GDI32.dll")>
    Private Shared Function SetPixelFormat(ByVal hnd As IntPtr, ByVal format As Int32, ByRef desc As PIXELFORMATDESCRIPTOR) As Boolean
    End Function

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glFlush()
    End Sub

    <DllImport("OpenGl32.dll")>
    Private Shared Sub glClearColor(r As Single, g As Single, b As Single, a As Single)
    End Sub

    <DllImport("User32.dll")>
    Private Shared Function GetDC(hnd As IntPtr) As IntPtr
    End Function

    <DllImport("User32.dll")>
    Private Shared Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("Opengl32.dll")>
    Private Shared Function wglCreateContext(hnd As IntPtr) As IntPtr
    End Function

    <DllImport("Opengl32.dll")>
    Private Shared Function wglMakeCurrent(hndDC As IntPtr, hndGl As IntPtr) As Boolean
    End Function

    <DllImport("Opengl32.dll")>
    Private Shared Sub glViewport(x As Int32, y As Int32, w As Int32, h As Int32)
    End Sub

    <DllImport("Opengl32.dll")>
    Private Shared Sub glClear(toClear As Int32)
    End Sub
End Class

Public Class Sound

    Structure SoundData
        Public LastPlayed As Dictionary(Of String, Single)
        Public counter As Integer
        Sub New(init As Boolean)
            LastPlayed = New Dictionary(Of String, Single)
            counter = 0
        End Sub
    End Structure

    Public Shared Sub PlaySound(location As String, Optional sync As Boolean = False)
        Dim ascii As New List(Of Byte)
        Dim ptr As IntPtr = Marshal.AllocHGlobal(1024)
        location = "Resource\" & location & ".wav"
        For i = 0 To location.Length - 1
            ascii.Add(CByte(Asc(location(i))))
        Next
        ascii.Add(0)
        Marshal.Copy(ascii.ToArray(), 0, ptr, ascii.Count)
        If sync Then
            PlaySound(ptr, 0, 0)
        Else
            PlaySound(ptr, 0, 9)
        End If
    End Sub

    Public Shared Sub CloseSound(id As Integer)
        mciSendString(StrToPtr("close " & id), IntPtr.Zero, 0, 0)
    End Sub

    Public Shared Sub PlayWalkSound(surface As String, ByRef soundData As Sound.SoundData)
        PlayAdditionalSound("walk" & surface, soundData)
    End Sub

    Public Shared Function StrToPtr(data As String) As IntPtr
        Dim retPtr As IntPtr = Marshal.AllocHGlobal(data.Length + 1)
        For i = 0 To data.Length - 1
            If data(i) = "@" Then
                Marshal.WriteByte(retPtr + i, 34)
            Else
                Marshal.WriteByte(retPtr + i, CByte(AscW(data(i))))
            End If
        Next
        Marshal.WriteByte(retPtr + data.Length, 0)
        Return retPtr
    End Function


    Public Shared Sub PlayAdditionalSound(location As String, ByRef soundData As SoundData)
        Dim proc As New Process
        Dim procInfo As New ProcessStartInfo

        'mciSendString(StrToPtr("open " & """\\?\" & My.Application.Info.DirectoryPath & "\Resource\SoundEffects\" & location & ".wav" & """" & " alias z"), IntPtr.Zero, 0, 0)
        'mciSendString(StrToPtr("open waveaudio!Resource\SoundEffects\" & location & ".wav alias z"), IntPtr.Zero, 0, 0)

        If IO.File.Exists("Resource\SoundEffects\" & location & ".wav") Then
            If Not soundData.LastPlayed.ContainsKey(location) Then
                soundData.LastPlayed.Add(location, 0)
            End If

            If Timer - soundData.LastPlayed(location) > 0.3F Then
                'procInfo.Arguments = "SoundEffects\" & location
                'procInfo.CreateNoWindow = True
                'procInfo.UseShellExecute = False
                'procInfo.FileName = Process.GetCurrentProcess.ProcessName
                'Process.Start(procInfo)
                soundData.LastPlayed(location) = CSng(Timer)
                mciSendString(StrToPtr("open ""Resource\SoundEffects\" & location & ".wav"" alias " & soundData.counter), IntPtr.Zero, 0, 0)
                mciSendString(StrToPtr("stop " & soundData.counter), IntPtr.Zero, 0, 0)
                mciSendString(StrToPtr("play " & soundData.counter), IntPtr.Zero, 0, 0)
                soundData.counter += 1

            End If
        End If
    End Sub

    <DllImport("Winmm.dll")>
    Private Shared Function PlaySound(loc As IntPtr, hnd As Int32, flags As Int32) As Boolean
    End Function

    <DllImport("Winmm.dll")>
    Private Shared Function mciSendString(command As IntPtr, retString As IntPtr, retSize As Int32, callBack As Int32) As Int32
    End Function
End Class

Public Class RenderWorld

    Public Shared NumTexturesBlocks As Integer
    Public Shared NumTexturesItems As Integer
    Public Shared NumTexturesTotal As Integer
    Public Shared ZombieTextureStart As Integer
    Public Shared facesType(3000000) As Byte
    Public Shared facesTypeWater(3000000) As Byte
    Public Shared totFaces As Integer
    Public Shared totFacesWater As Integer
    Public Shared coords(10000000) As Single
    Public Shared coordsWater(10000000) As Single
    Public Shared lighting(10000000) As Byte
    Public Shared lightingWater(10000000) As Byte
    Public Shared OldPlayerX As Integer
    Public Shared OldPlayerZ As Integer
    Public Shared ChunksDataIndex(RENDERDISTANCE * 2, RENDERDISTANCE * 2) As Integer
    Public Shared LoadedChunks(RENDERDISTANCE * RENDERDISTANCE * 10) As ChunkFaces
    Public Shared GenerateBlockList As New List(Of ChunkCoord)
    Public Shared GeneratePostBlockList As New List(Of ChunkCoord)
    Public Shared GenerateLightingNaturalList As New List(Of ChunkCoord)
    Public Shared GenerateLightingArtificialList As New List(Of ChunkCoord)
    Public Shared GenerateSourceList As New List(Of ChunkCoord)
    Public Shared GenerateFaceList As New List(Of ChunkCoord)
    Public Shared okGen As Integer
    Public Shared okGenNat As Integer
    Public Shared fpsDrop As Boolean
    Public Shared NaturalLight As Single
    Public Shared TorchesToRender As List(Of CustomBlocks.COORD3)
    Public Shared TorchOrientationsToRender As List(Of CustomBlocks.TorchBlockAttached)
    Public Shared AllTorchData As New List(Of TorchData)
    Public Shared BlocksToRelightChunks As ChunkCoord
    Public Shared BlocksToRelightBlocks(,) As List(Of Integer)
    Public Shared BlocksToRelightDepthNatural As Integer
    Public Shared BlocksToRelightDepthArtificial As Integer
    '^Sneaky globals :P

    Public Structure TorchData
        Public chunk As ChunkCoord
        Public location As Integer
        Public orientation As CustomBlocks.TorchBlockAttached
        Sub New(reload As Boolean)
            chunk = New ChunkCoord()
        End Sub
    End Structure

    Public Structure ChunkCoord
        Public x As Integer
        Public z As Integer
        Sub New(inx As Integer, inz As Integer)
            x = inx
            z = inz
        End Sub
    End Structure

    Private Shared Function DisplayMask(size As Integer, rotation As Single, offset As Single) As Boolean(,)
        Dim mask(size * 2, size * 2) As Boolean
        Dim start As Single
        Dim rotAdjusted As Single = rotation + offset
        If rotAdjusted > Math.PI * 2 Then rotAdjusted -= CSng(Math.PI * 2)
        For i = 0 To size * 2 - 1
            start = CSng(-Math.Tan(rotAdjusted) * (size - i) + size)
            If start < 1 Then start = 1
            If start > size * 2 - 2 Then start = size * 2 - 2
            If rotAdjusted < Math.PI * 2 / 4 Or rotAdjusted > Math.PI * 6 / 4 Then
                For j = CInt(Math.Floor(start - 1)) To CInt(size * 2 - 1)
                    mask(j, i) = True
                Next
            Else
                For j = 0 To CInt(Math.Ceiling(start + 1))
                    mask(j, i) = True
                Next
            End If
        Next
        Return mask
    End Function

    Private Shared Function MaskAnd(arr1 As Boolean(,), arr2 As Boolean(,)) As Boolean(,)
        For i = 0 To arr1.GetLength(0) - 1
            For j = 0 To arr1.GetLength(1) - 1
                arr1(i, j) = arr1(i, j) And arr2(i, j)
            Next
        Next
        Return arr1
    End Function

    Public Shared Function DaytimeToDaylight(daytime As Single) As Single
        If daytime < 0.2 Then Return 0.05
        If daytime > 0.8 Then Return 0.05
        If daytime < 0.7 And daytime > 0.3 Then Return 1
        If daytime < 0.5 Then Return (daytime - 0.2F) * 9.5F + 0.05F
        Return (0.8F - daytime) * 9.5F + 0.05F
    End Function

    Public Shared Sub RenderAllChunks(daylight As Single, water As Boolean)
        Dim shouldRender(RENDERDISTANCE * 2, RENDERDISTANCE * 2) As Boolean
        Dim preloaded As Byte() = PreLoadLighting(DaytimeToDaylight(daylight))
        shouldRender = MaskAnd(DisplayMask(RENDERDISTANCE, Player.playerAngle, Math.PI * 5.5 / 4), DisplayMask(RENDERDISTANCE, Player.playerAngle, Math.PI * 6.5 / 4))

        TorchesToRender.Clear()
        TorchOrientationsToRender.Clear()

        For i = 1 To RENDERDISTANCE * 2 - 1
            For j = 1 To RENDERDISTANCE * 2 - 1
                If ChunksDataIndex(i, j) <> -1 And shouldRender(i, j) Then
                    AddChunkToRender(LoadedChunks(ChunksDataIndex(i, j)), daylight, water, preloaded)
                End If
            Next
        Next
    End Sub

    Private Shared Function PreLoadLighting(daylight As Single) As Byte()
        Dim preload As New List(Of Byte)
        Dim newLightLevel As Byte
        For i = 0 To 15 ' MSB is natural light
            For j = 0 To 15
                newLightLevel = CByte(i * i * daylight + 20)
                If j * j + 20 > newLightLevel Then
                    newLightLevel = CByte(j * j + 20)
                End If
                preload.Add(newLightLevel)
            Next
        Next
        Return preload.ToArray()
    End Function

    Public Shared c As Integer
    Public Shared count As Integer

    Private Shared Function PriorityChunk(ByRef chunkList As List(Of ChunkCoord)) As Integer
        Dim minDistance As Integer = 1000
        Dim distance As Integer
        Dim minIndex As Integer = -1
        For i = 0 To chunkList.Count - 1
            distance = (Player.chunkX - chunkList(i).x) * (Player.chunkX - chunkList(i).x) + (Player.chunkZ - chunkList(i).z) * (Player.chunkZ - chunkList(i).z)
            If distance < minDistance Then
                minDistance = distance
                minIndex = i
            End If
        Next
        Return minIndex
    End Function

    Public Shared Sub UpdateChunks(blockData As ImportedData.BlockData(), allChanges As FEN.ChunkChanges(), ByRef randoms As Single())
        Dim t As Double = Timer
        Dim currentChange As List(Of FEN.BlockChange)
        Dim focusIndex As Integer
        c = 0
        If Player.PlacedAtTime = 0 Then
            count = 0
        End If
        While GenerateFaceList.Count > 0 And (Timer - t < 0.01 Or (fpsDrop And Timer - t < 0.03 And False))
            If GenerateBlockList.Count > 0 Then
                focusIndex = PriorityChunk(GenerateBlockList)
                LoadMoreChunksBlock(GenerateBlockList(focusIndex), focusIndex, randoms)
            ElseIf GeneratePostBlockList.Count > 0 Then
                focusIndex = PriorityChunk(GeneratePostBlockList)
                currentChange = FEN.GetChangesFromChunk(GeneratePostBlockList(focusIndex), allChanges)
                LoadMoreChunksPostBlock(GeneratePostBlockList(focusIndex), currentChange, focusIndex, randoms)
            ElseIf GenerateLightingNaturalList.Count > 0 Then
                focusIndex = PriorityChunk(GenerateLightingNaturalList)
                NaturalLightGeneration(GenerateLightingNaturalList(focusIndex), focusIndex)
            ElseIf GenerateLightingArtificialList.Count > 0 Then
                focusIndex = PriorityChunk(GenerateLightingArtificialList)
                ArtificialLightGeneration(GenerateLightingArtificialList(focusIndex), focusIndex)
            ElseIf GenerateFaceList.Count > 0 Then
                focusIndex = PriorityChunk(GenerateFaceList)
                If okGen < GenerateFaceList.Count And False Then
                    'LoadMoreChunksFaces(GenerateFaceList(okGen), blockData)
                Else
                    LoadMoreChunksFaces(GenerateFaceList(focusIndex), blockData, focusIndex)
                End If
            End If
            c += 1
        End While
        If GenerateFaceList.Count = 0 Then
            If Player.PlacedAtTime > 0 Then
                'SetCursorPosition(0, 0)
                'WriteLine(Timer - Player.PlacedAtTime & " " & count)
                'Threading.Thread.Sleep(2000)
                Player.PlacedAtTime = 0
            End If
            fpsDrop = False
            RefreshBadChunks()
            GenerateSourceList.Clear()
        End If
        If OldPlayerX <> Player.chunkX Or OldPlayerZ <> Player.chunkZ Then
            ChunkIndexReload()
            BorderChunkReload()
            OldPlayerX = Player.chunkX
            OldPlayerZ = Player.chunkZ
        End If
    End Sub

    Private Shared Sub RefreshBadChunks()
        Dim i As Integer = 0
        Dim toRefresh As Integer = -1
        While i < LoadedChunks.Length And toRefresh = -1
            If LoadedChunks(i).inUse And Not LoadedChunks(i).placed Then
                toRefresh = i
            End If
            i += 1
        End While
        If toRefresh > -1 Then
            LoadedChunks(toRefresh).placed = True
            GenerateFaceList.Add(New ChunkCoord(LoadedChunks(toRefresh).x, LoadedChunks(toRefresh).z))
        End If
    End Sub

    Public Shared Sub RawDataOutput()
        Dim rawDataTest As New Text.StringBuilder
        For i = 30 To 40
            For j = 0 To LoadedChunks(i).data.Length - 1
                rawDataTest.Append("," & LoadedChunks(i).data(j))
            Next
        Next
        IO.File.WriteAllText("info.txt", rawDataTest.ToString)

    End Sub

    Public Shared Sub RenderWorld(ByVal daylight As Single, ByRef blocks As ImportedData.BlockData(), ByRef zombies As Zombie(), ByRef openglData As OpenGL.OpenGlData)
        daylight = DaytimeToDaylight(daylight)
        OpenGL.Clear(daylight / 4, daylight / 4, daylight)
        OpenGL.InitTextures(facesType, NumTexturesTotal, totFaces, New List(Of OpenGL.FractionalIcon), openglData)
        OpenGL.RenderScene(Player.playerAngle, Player.playerElevation, coords, totFaces, lighting, openglData)
        OpenGL.InitTextures(facesTypeWater, NumTexturesTotal, totFacesWater + totFaces, New List(Of OpenGL.FractionalIcon), openglData)
        OpenGL.RenderTransluscentBlock(coordsWater, lightingWater, openglData)
        RenderTransparentBlocks(blocks, zombies, openglData)
    End Sub

    Private Shared Sub RenderTransparentBlocks(ByRef blocks As ImportedData.BlockData(), ByRef zombies As Zombie(), ByRef openGlData As OpenGL.OpenGlData)
        Dim transparentData As New List(Of Single)
        Dim transparentTextures As New List(Of Byte)
        Dim transparentColours As New List(Of Byte)
        Dim crackTexture As Integer = CInt(NumTexturesBlocks + NumTexturesItems + 27 + Math.Floor(Player.MiningProgress * 10 - 0.01))

        For i = 0 To TorchesToRender.Count - 1
            CustomBlocks.Torch(TorchOrientationsToRender(i), TorchesToRender(i), transparentData, transparentTextures, transparentColours, blocks)
        Next

        If Player.MiningProgress = 0 Then
            crackTexture = NumTexturesBlocks + NumTexturesItems + 77
        End If

        If Not Player.targetBlock.failed Then
            For i = 0 To 5
                transparentTextures.Add(CByte(crackTexture))
            Next
            transparentData.AddRange(GetFacesOfBlock(Math.Floor(Player.targetBlock.x), CInt(Math.Floor(Player.targetBlock.y)), Math.Floor(Player.targetBlock.z), 0, 0))
        End If

        While transparentColours.Count < transparentTextures.Count * 12
            transparentColours.Add(255)
        End While

        For i = 0 To zombies.Length - 1
            If zombies(i).inUse Then
                zombies(i).GenerateFaces(transparentTextures, transparentData, transparentColours)
            End If
        Next

        OpenGL.InitTextures(transparentTextures.ToArray(), NumTexturesTotal, transparentTextures.Count, New List(Of OpenGL.FractionalIcon), openGlData)
        OpenGL.RenderTransparentBlock(NumTexturesTotal, transparentData.ToArray(), transparentColours.ToArray(), openGlData)
    End Sub

    Public Shared Function GetBlock(x As Integer, y As Integer, z As Integer, Optional debugMode As Boolean = False) As Byte
        Dim chunkX As Integer = CoordsToChunk(x)
        Dim chunkZ As Integer = CoordsToChunk(z)
        Dim index As Integer = GetIndexOfChunkData(chunkX, chunkZ)
        If index = -1 Then Return 0
        If y < 1 Then Return 0
        If y > 254 Then Return 0
        Return LoadedChunks(index).data(GetRelPositionOfBlock(x, y, z))
    End Function

    Public Shared Sub GetLightBlock(x As Integer, y As Integer, z As Integer, ByRef art As Byte, ByRef nat As Byte, Optional debugMode As Boolean = False)
        Dim chunkX As Integer = CoordsToChunk(x)
        Dim chunkZ As Integer = CoordsToChunk(z)
        Dim index As Integer = GetIndexOfChunkData(chunkX, chunkZ)
        art = 10
        nat = 10
        If index = -1 Then Return
        If y < 1 Then Return
        If y > 254 Then Return
        art = LoadedChunks(index).lightArtificial(GetRelPositionOfBlock(x, y, z))
        nat = LoadedChunks(index).lightNatural(GetRelPositionOfBlock(x, y, z))
    End Sub


    Public Shared Function GetRelPositionOfBlock(x As Integer, y As Integer, z As Integer) As Integer
        Return CoordsToBlockRelative(x) + y * 16 + (CoordsToBlockRelative(z)) * 4096
    End Function

    Public Shared Function CoordsToBlockRelative(coord As Integer) As Integer
        Dim chunk As Integer = CoordsToChunk(coord)
        Return coord - chunk * 16
    End Function

    Public Shared Function CoordsToChunk(coord As Integer) As Integer
        Return CInt(Math.Floor(coord / 16))
    End Function

    Public Shared Sub AddChunkToRender(chunk As ChunkFaces, daylight As Single, water As Boolean, ByRef lightingPreloaded As Byte())
        Dim numFaces As Integer = 0
        Dim north, east, south, west, up, down As ChunkFace
        If water Then
            north = chunk.northWater
            south = chunk.southWater
            east = chunk.eastWater
            west = chunk.westWater
            up = chunk.upWater
            down = chunk.downWater
        Else
            If chunk.redirect Then
                north = chunk.redirectTarget(2)
                south = chunk.redirectTarget(4)
                east = chunk.redirectTarget(3)
                west = chunk.redirectTarget(5)
                up = chunk.redirectTarget(0)
                down = chunk.redirectTarget(1)
            Else
                north = chunk.north
                south = chunk.south
                east = chunk.east
                west = chunk.west
                up = chunk.up
                down = chunk.down
            End If
        End If
        If chunk.redirect Then
            For i = 0 To chunk.artificialLightLocationsRedirect.Count - 1
                TorchesToRender.Add(CustomBlocks.ChunkToAbsoluteWorldCoord(chunk.artificialLightLocationsRedirect(i), New ChunkCoord(chunk.x, chunk.z)))
                TorchOrientationsToRender.Add(chunk.artificialLightOrientationsRedirect(i))
            Next
        Else
            For i = 0 To chunk.artificialLightLocations.Count - 1
                TorchesToRender.Add(CustomBlocks.ChunkToAbsoluteWorldCoord(chunk.artificialLightLocations(i), New ChunkCoord(chunk.x, chunk.z)))
                TorchOrientationsToRender.Add(chunk.artificialLightOrientations(i))
            Next
        End If
        If Player.chunkX >= chunk.x Then ProcessFaceDirection(east, daylight, water, lightingPreloaded)
        If Player.chunkX <= chunk.x Then ProcessFaceDirection(west, daylight, water, lightingPreloaded)
        If Player.chunkZ >= chunk.z Then ProcessFaceDirection(north, daylight, water, lightingPreloaded)
        If Player.chunkZ <= chunk.z Then ProcessFaceDirection(south, daylight, water, lightingPreloaded)
        ProcessFaceDirection(up, daylight, water, lightingPreloaded)
        ProcessFaceDirection(down, daylight, water, lightingPreloaded)

    End Sub

    Private Shared Sub ProcessFaceDirection(faces As ChunkFace, daylight As Single, water As Boolean, ByRef lightingPreloaded As Byte())
        Dim numFaces As Integer
        numFaces = faces.blockType.Length
        LightingClass.MergeLighting(daylight, faces, lightingPreloaded)
        If water Then
            If numFaces > 0 Then
                numFaces = numFaces
            End If
            Array.Copy(faces.blockType, 0, facesTypeWater, totFacesWater, numFaces)
            Array.Copy(faces.coords, 0, coordsWater, totFacesWater * 16, numFaces * 16)
            For i = 0 To numFaces * 4 - 1
                lightingWater(i * 4 + totFacesWater * 16) = faces.lightFinal(i * 3)
                lightingWater(i * 4 + 1 + totFacesWater * 16) = faces.lightFinal(i * 3 + 1)
                lightingWater(i * 4 + 2 + totFacesWater * 16) = faces.lightFinal(i * 3 + 2)
                lightingWater(i * 4 + 3 + totFacesWater * 16) = 200
            Next
            totFacesWater += numFaces
        Else
            Array.Copy(faces.blockType, 0, facesType, totFaces, numFaces)
            Array.Copy(faces.coords, 0, coords, totFaces * 16, numFaces * 16)
            Array.Copy(faces.lightFinal, 0, lighting, totFaces * 12, numFaces * 12)
            totFaces += numFaces
        End If
    End Sub

    Private Enum Shift
        left = 0
        up = 1
        right = 2
        down = 3
    End Enum

    Public Shared Sub Initialise(blockData As ImportedData.BlockData(), ByRef blockChanges As FEN.ChunkChanges(), ByRef randoms As Single(), ByRef OpenGlData As OpenGL.OpenGlData)
        Dim t As Double = Timer
        Dim elapsedTime As Integer = 0
        Dim chunkIndex As Integer
        Dim currentX, currentZ As Integer
        Dim offsetX, offsetZ As Integer
        Dim surround(2, 2)() As Terrain.TreeLocation
        Dim currentChunk As ChunkFaces
        Dim currentChunkCoord As New ChunkCoord
        Dim changes As List(Of FEN.BlockChange)
        Dim surroundChunks(2, 2) As ChunkFaces

        Loading_Screen.LoadScreenInit()
        TorchesToRender = New List(Of CustomBlocks.COORD3)
        TorchOrientationsToRender = New List(Of CustomBlocks.TorchBlockAttached)

        For i = 0 To LoadedChunks.Length - 1
            LoadedChunks(i).hasData = False
            LoadedChunks(i).hasFaces = False
            LoadedChunks(i).inUse = False
            LoadedChunks(i).artificialLightLocations = New List(Of Integer)
            LoadedChunks(i).artificialLightOrientations = New List(Of CustomBlocks.TorchBlockAttached)
            LoadedChunks(i).toSpreadArtificial = LightingClass.NewSpreadStructure()
            LoadedChunks(i).toSpreadNatural = LightingClass.NewSpreadStructure()
            LoadedChunks(i).overflowArtificialSaved = LightingClass.NewSpreadStructure()
            LoadedChunks(i).overflowNaturalSaved = LightingClass.NewSpreadStructure()
            LoadedChunks(i).placed = False
            LoadedChunks(i).redirect = False
            LoadedChunks(i).redirectTarget = New List(Of ChunkFace)
            LoadedChunks(i).artificialLightLocationsRedirect = New List(Of Integer)
            LoadedChunks(i).artificialLightOrientationsRedirect = New List(Of CustomBlocks.TorchBlockAttached)
            LoadedChunks(i).north = New ChunkFace(True)
            LoadedChunks(i).south = New ChunkFace(True)
            LoadedChunks(i).east = New ChunkFace(True)
            LoadedChunks(i).west = New ChunkFace(True)
            LoadedChunks(i).up = New ChunkFace(True)
            LoadedChunks(i).down = New ChunkFace(True)
            LoadedChunks(i).additionalLight = False
        Next

        ReDim BlocksToRelightBlocks(2, 2)
        For i = 0 To 2
            For j = 0 To 2
                BlocksToRelightBlocks(i, j) = New List(Of Integer)
            Next
        Next

        offsetX = Player.chunkX - RENDERDISTANCE
        offsetZ = Player.chunkZ - RENDERDISTANCE

        For i = 0 To RENDERDISTANCE * 2
            For j = 0 To RENDERDISTANCE * 2
                chunkIndex = i * (RENDERDISTANCE * 2 + 1) + j
                LoadedChunks(chunkIndex).x = offsetX + i
                LoadedChunks(chunkIndex).z = offsetZ + j
                Terrain.GenerateChunk(LoadedChunks(chunkIndex).x, LoadedChunks(chunkIndex).z, LoadedChunks(chunkIndex).data, LoadedChunks(chunkIndex).treeLocs, randoms)
                LoadedChunks(chunkIndex).hasData = True
                LoadedChunks(chunkIndex).hasFaces = False
                If Timer - t > 0.1 Then
                    t += 0.1
                    Loading_Screen.LoadScreen(elapsedTime Mod 18)
                    elapsedTime += 1
                End If
            Next
        Next

        For i = 0 To LoadedChunks.Length - 1
            LightingClass.InitialiseToSpread(LoadedChunks(i).toSpreadNatural)
            LightingClass.InitialiseToSpread(LoadedChunks(i).toSpreadArtificial)
            LightingClass.InitialiseBlockLighting(LoadedChunks(i).lightNatural)
            LightingClass.InitialiseBlockLighting(LoadedChunks(i).lightArtificial)
        Next
        For i = 1 To RENDERDISTANCE * 2 - 1
            For j = 1 To RENDERDISTANCE * 2 - 1
                For k = 0 To 2
                    For l = 0 To 2
                        surround(k, l) = LoadedChunks((i + k - 1) * (RENDERDISTANCE * 2 + 1) + (j + l - 1)).treeLocs
                    Next
                Next
                currentChunk = LoadedChunks(i * (RENDERDISTANCE * 2 + 1) + j)
                changes = FEN.GetChangesFromChunk(New ChunkCoord(currentChunk.x, currentChunk.z), blockChanges)
                Terrain.PostGeneration(currentChunk.data, surround, currentChunk.x * 1000 + currentChunk.z, changes, randoms)
                currentChunkCoord = New ChunkCoord(currentChunk.x, currentChunk.z)
                LightingClass.GenerateNaturalBlockLighting(currentChunk.data, currentChunk.lightNatural, GetSurroundIndex(currentChunkCoord), currentChunk.toSpreadNatural, currentChunk.isLitNatural, GetSurroundIndexFull(currentChunkCoord), currentChunkCoord)
                LightingClass.GenerateArtificialBlockLighting(currentChunk.data, currentChunk.lightArtificial, currentChunk.artificialLightLocations, GetSurroundIndex(currentChunkCoord), currentChunk.toSpreadArtificial, currentChunk.isLitArtificial, GetSurroundIndexFull(currentChunkCoord), currentChunk.additionalLight, currentChunkCoord)
                If Timer - t > 0.1 Then
                    t += 0.1
                    Loading_Screen.LoadScreen(elapsedTime Mod 18)
                    elapsedTime += 1
                End If
                LoadTorches(currentChunk.artificialLightLocations, currentChunk.artificialLightOrientations, currentChunk.x, currentChunk.z)
            Next
        Next

        For i = 2 To RENDERDISTANCE * 2 - 2
            For j = 2 To RENDERDISTANCE * 2 - 2
                currentX = offsetX + i
                currentZ = offsetZ + j
                GetSurroundChunks(surroundChunks, New ChunkCoord(currentX, currentZ))
                GetFaces(surroundChunks, blockData, False)
                GetFaces(surroundChunks, blockData, True)
                LoadedChunks(GetIndexOfChunkData(currentX, currentZ)) = surroundChunks(1, 1)
                LoadedChunks(GetIndexOfChunkData(currentX, currentZ)).hasFullData = True
                LoadedChunks(GetIndexOfChunkData(currentX, currentZ)).hasFaces = True
                LoadedChunks(GetIndexOfChunkData(currentX, currentZ)).additionalLight = True
                GenerateFaceList.Add(New ChunkCoord(currentX, currentZ))
            Next
        Next

        ChunkIndexReload()

        OldPlayerX = Player.chunkX
        OldPlayerZ = Player.chunkZ
        OpenGL.InitTextures(facesType, NumTexturesTotal, totFaces, New List(Of OpenGL.FractionalIcon), OpenGlData)
        While elapsedTime Mod 18 <> 1
            If Timer - t > 0.1 Then
                t += 0.1
                Loading_Screen.LoadScreen(elapsedTime Mod 18)
                elapsedTime += 1
            End If
        End While

    End Sub

    Private Shared Function GetSurroundIndex(coordinates As ChunkCoord) As Integer()
        Dim surround(3) As Integer
        surround(0) = GetIndexOfChunkData(coordinates.x, coordinates.z - 1)
        surround(1) = GetIndexOfChunkData(coordinates.x, coordinates.z + 1)
        surround(2) = GetIndexOfChunkData(coordinates.x - 1, coordinates.z)
        surround(3) = GetIndexOfChunkData(coordinates.x + 1, coordinates.z)
        Return surround
    End Function

    Private Shared Sub GetSurroundChunks(ByRef surround(,) As ChunkFaces, ByRef centre As ChunkCoord)
        Dim surroundIndex As Integer(,) = GetSurroundIndexFull(centre)
        For i = 0 To 2
            For j = 0 To 2
                If surroundIndex(i, j) > -1 Then
                    surround(i, j) = LoadedChunks(surroundIndex(i, j))
                Else
                    surround(i, j) = LoadedChunks(0)
                End If
            Next
        Next
    End Sub

    Private Shared Function GetSurroundIndexFull(coordinates As ChunkCoord) As Integer(,)
        Dim surround(2, 2) As Integer
        For i = 0 To 2
            For j = 0 To 2
                surround(i, j) = GetIndexOfChunkData(coordinates.x + i - 1, coordinates.z + j - 1)
            Next
        Next
        Return surround
    End Function

    Public Shared Sub LoadMoreChunksBlock(coordinates As ChunkCoord, targetIndex As Integer, ByRef randoms As Single())
        Dim index As Integer = 0
        Dim exitNow As Boolean = False
        Dim distance, maxDistance As Integer
        While LoadedChunks(index).inUse And Not exitNow
            index = index + 1
            If index = LoadedChunks.Length Then exitNow = True
        End While
        distance = 0
        maxDistance = 0
        If exitNow Then
            For i = 0 To LoadedChunks.Length - 1
                distance = CInt((Player.chunkX - LoadedChunks(i).x) ^ 2 + (Player.chunkZ - LoadedChunks(i).z) ^ 2)
                If distance > maxDistance Then
                    maxDistance = distance
                    index = i
                End If
            Next
        End If

        If LoadedChunks(index).inUse Then
            BackupTorches(LoadedChunks(index).artificialLightLocations, LoadedChunks(index).artificialLightOrientations, New ChunkCoord(LoadedChunks(index).x, LoadedChunks(index).z))
        End If

        LoadedChunks(index).x = coordinates.x
        LoadedChunks(index).z = coordinates.z
        LoadedChunks(index).inUse = True
        LoadedChunks(index).hasData = True
        LoadedChunks(index).hasFullData = False
        LoadedChunks(index).hasFaces = False
        LoadedChunks(index).isLitArtificial = False
        LoadedChunks(index).isLitNatural = False
        LoadedChunks(index).placed = False

        LoadTorches(LoadedChunks(index).artificialLightLocations, LoadedChunks(index).artificialLightOrientations, coordinates.x, coordinates.z)

        Terrain.GenerateChunk(coordinates.x, coordinates.z, LoadedChunks(index).data, LoadedChunks(index).treeLocs, randoms)
        GenerateBlockList.RemoveAt(targetIndex)
    End Sub

    Private Shared Sub LoadTorches(ByRef location As List(Of Integer), ByRef orientation As List(Of CustomBlocks.TorchBlockAttached), chunkX As Integer, chunkZ As Integer)
        For i = 0 To AllTorchData.Count - 1
            If AllTorchData(i).chunk.x = chunkX AndAlso AllTorchData(i).chunk.z = chunkZ Then
                location.Add(AllTorchData(i).location)
                orientation.Add(AllTorchData(i).orientation)
            End If
        Next
    End Sub

    Public Shared Sub BackupTorches(location As List(Of Integer), orientation As List(Of CustomBlocks.TorchBlockAttached), chunk As ChunkCoord)
        Dim i As Integer = 0
        Dim newTorchData As New TorchData(True)
        While i < AllTorchData.Count
            If AllTorchData(i).chunk.x = chunk.x AndAlso AllTorchData(i).chunk.z = chunk.z Then
                AllTorchData.RemoveAt(i)
            Else
                i += 1
            End If
        End While
        For i = 0 To location.Count - 1
            newTorchData.chunk.x = chunk.x
            newTorchData.chunk.z = chunk.z
            newTorchData.location = location(i)
            newTorchData.orientation = orientation(i)
            AllTorchData.Add(newTorchData)
        Next
    End Sub

    Public Shared Function GetSurroundTrees(chunkX As Integer, chunkZ As Integer) As Terrain.TreeLocation(,)()
        Dim surround(2, 2)() As Terrain.TreeLocation
        For i = 0 To 2
            For j = 0 To 2
                surround(i, j) = LoadedChunks(GetIndexOfChunkData(chunkX + i - 1, chunkZ + j - 1)).treeLocs
            Next
        Next
        Return surround
    End Function

    Private Shared Sub NaturalLightGeneration(coordinates As ChunkCoord, targetIndex As Integer)
        Dim index As Integer = 0
        index = GetIndexOfChunkData(coordinates.x, coordinates.z)
        If index > -1 Then
            If LoadedChunks(index).hasFullData Then
                LoadedChunks(index).overflowNatural = False
                LightingClass.GenerateNaturalBlockLighting(LoadedChunks(index).data, LoadedChunks(index).lightNatural, GetSurroundIndex(coordinates), LoadedChunks(index).toSpreadNatural, Not LoadedChunks(index).isLitNatural, GetSurroundIndexFull(coordinates), coordinates)
                If Not LoadedChunks(index).isLitNatural Then
                    index = index
                End If
                LoadedChunks(index).isLitNatural = True
                GenerateLightingNaturalList.RemoveAt(targetIndex)
                count += 1
            Else
                If Not DuplicateCoordsToGenerate(coordinates, GeneratePostBlockList) Then
                    GeneratePostBlockList.Add(coordinates)
                End If
            End If
        Else
            If Not DuplicateCoordsToGenerate(coordinates, GenerateBlockList) Then
                GenerateBlockList.Add(coordinates)
            End If
        End If
    End Sub

    Private Shared Sub ArtificialLightGeneration(coordinates As ChunkCoord, targetIndex As Integer)
        Dim index As Integer = 0
        Dim surroundCoords As New ChunkCoord
        Dim surroundIndex As Integer
        index = GetIndexOfChunkData(coordinates.x, coordinates.z)
        For i = -1 To 1
            For j = -1 To 1
                index = GetIndexOfChunkData(coordinates.x + i, coordinates.z + j)
                If index = -1 Then
                    If Not DuplicateCoordsToGenerate(New ChunkCoord(coordinates.x + i, coordinates.z + j), GenerateBlockList) Then
                        GenerateBlockList.Add(New ChunkCoord(coordinates.x + i, coordinates.z + j))
                    Else
                        If LoadedChunks(index).overflowNatural Or Not LoadedChunks(index).isLitNatural Then
                            If Not DuplicateCoordsToGenerate(New ChunkCoord(coordinates.x + i, coordinates.z + j), GenerateLightingNaturalList) Then
                                GenerateLightingNaturalList.Add(New ChunkCoord(coordinates.x + i, coordinates.z + j))
                            End If
                        End If
                    End If
                End If
            Next
        Next
        index = GetIndexOfChunkData(coordinates.x, coordinates.z)
        If index > -1 Then
            If LoadedChunks(index).isLitNatural Then
                LoadedChunks(index).overflowArtificial = False
                LightingClass.GenerateArtificialBlockLighting(LoadedChunks(index).data, LoadedChunks(index).lightArtificial, LoadedChunks(index).artificialLightLocations, GetSurroundIndex(coordinates), LoadedChunks(index).toSpreadArtificial, Not LoadedChunks(index).isLitArtificial, GetSurroundIndexFull(coordinates), LoadedChunks(index).additionalLight, coordinates)
                LoadedChunks(index).isLitArtificial = True
                GenerateLightingArtificialList.RemoveAt(targetIndex)
                LoadedChunks(index).isSmooth = False
                For i = 0 To 1 ' Update surrounding chunks if necessary
                    For j = 0 To 1
                        surroundCoords.x = coordinates.x + i
                        surroundCoords.z = coordinates.z + j
                        surroundIndex = GetIndexOfChunkData(surroundCoords.x, surroundCoords.z)
                        If surroundIndex > -1 Then
                            LoadedChunks(surroundIndex).isSmooth = False
                        End If
                    Next
                Next
            Else
                If Not DuplicateCoordsToGenerate(coordinates, GenerateLightingNaturalList) Then
                    GenerateLightingNaturalList.Add(coordinates)
                End If
            End If
        Else
            If Not DuplicateCoordsToGenerate(coordinates, GenerateBlockList) Then
                GenerateBlockList.Add(coordinates)
            End If
        End If
    End Sub

    Public Shared Sub LoadMoreChunksPostBlock(coordinates As ChunkCoord, blockChanges As List(Of FEN.BlockChange), targetIndex As Integer, ByRef randoms As Single())
        Dim index As Integer = 0
        Dim canLoad As Boolean = True
        Dim isSurrounded As Boolean = True
        For i = -1 To 1
            For j = -1 To 1
                If GetIndexOfChunkData(coordinates.x + i, coordinates.z + j) = -1 Then
                    canLoad = False
                    If Not DuplicateCoordsToGenerate(New ChunkCoord(coordinates.x + i, coordinates.z + j), GenerateBlockList) Then
                        GenerateBlockList.Add(New ChunkCoord(coordinates.x + i, coordinates.z + j))
                    End If
                End If
            Next
        Next
        If canLoad Then
            index = GetIndexOfChunkData(coordinates.x, coordinates.z)
            LoadedChunks(index).inUse = True
            LoadedChunks(index).hasData = True
            LoadedChunks(index).hasFullData = True
            LoadedChunks(index).hasFaces = False
            Terrain.PostGeneration(LoadedChunks(index).data, GetSurroundTrees(LoadedChunks(index).x, LoadedChunks(index).z), LoadedChunks(index).x * 1000 + LoadedChunks(index).z, blockChanges, randoms)
            GeneratePostBlockList.RemoveAt(targetIndex)
            For i = -1 To 1
                For j = -1 To 1
                    index = GetIndexOfChunkData(coordinates.x, coordinates.z)
                    If index = -1 Then
                        isSurrounded = False
                    End If
                Next
            Next
            If Not DuplicateCoordsToGenerate(coordinates, GenerateSourceList) Then
                GenerateSourceList.Add(coordinates)
            End If
        End If
    End Sub

    Public Shared Sub LoadMoreChunksFaces(coordinates As ChunkCoord, blockData As ImportedData.BlockData(), targetIndex As Integer)
        Dim x As Integer = coordinates.x
        Dim z As Integer = coordinates.z
        Dim canLoad As Boolean = True
        Dim redirectTarget As New ChunkFace
        Dim surroundChunks(2, 2) As ChunkFaces
        If GetIndexOfChunkData(x, z) = -1 OrElse Not LoadedChunks(GetIndexOfChunkData(x, z)).isLitArtificial OrElse LoadedChunks(GetIndexOfChunkData(x, z)).overflowArtificial Then
            canLoad = False
            If Not DuplicateCoordsToGenerate(New ChunkCoord(x, z), GenerateLightingArtificialList) Then
                GenerateLightingArtificialList.Add(New ChunkCoord(x, z))
            End If
        End If
        For i = -1 To 1
            For j = -1 To 1
                If GetIndexOfChunkData(x + i, z + j) = -1 Then
                    canLoad = False
                    If Not DuplicateCoordsToGenerate(New ChunkCoord(x + i, z + j), GenerateBlockList) Then
                        GenerateBlockList.Add(New ChunkCoord(x + i, z + j))
                    End If
                Else
                    If LoadedChunks(GetIndexOfChunkData(x + i, z + j)).overflowArtificial Then
                        If Not DuplicateCoordsToGenerate(New ChunkCoord(x + i, z + j), GenerateLightingArtificialList) Then
                            GenerateLightingArtificialList.Add(New ChunkCoord(x + i, z + j))
                        End If
                    End If
                End If
            Next
        Next
        If canLoad Then
            If okGen >= GenerateFaceList.Count Or True Then
                GetSurroundChunks(surroundChunks, New ChunkCoord(x, z))
                GetFaces(surroundChunks, blockData, False)
                GetFaces(surroundChunks, blockData, True)
                LoadedChunks(GetIndexOfChunkData(x, z)) = surroundChunks(1, 1)
                LoadedChunks(GetIndexOfChunkData(x, z)).hasFaces = True
                LoadedChunks(GetIndexOfChunkData(x, z)).redirect = False
                LoadedChunks(GetIndexOfChunkData(x, z)).redirectTarget.Clear()
                LoadedChunks(GetIndexOfChunkData(x, z)).artificialLightOrientationsRedirect.Clear()
                LoadedChunks(GetIndexOfChunkData(x, z)).artificialLightLocationsRedirect.Clear()
                GenerateFaceList.RemoveAt(targetIndex)
                ChunkIndexReload()
            Else
                okGen += 1
            End If
        Else 'Make temp copy
            okGen = 0
            If GetIndexOfChunkData(x, z) > -1 Then
                LoadedChunks(GetIndexOfChunkData(x, z)).redirectTarget.Clear()
                LoadedChunks(GetIndexOfChunkData(x, z)).artificialLightOrientationsRedirect.Clear()
                LoadedChunks(GetIndexOfChunkData(x, z)).artificialLightLocationsRedirect.Clear()
                LoadedChunks(GetIndexOfChunkData(x, z)).artificialLightOrientationsRedirect.AddRange(LoadedChunks(GetIndexOfChunkData(x, z)).artificialLightOrientations)
                LoadedChunks(GetIndexOfChunkData(x, z)).artificialLightLocationsRedirect.AddRange(LoadedChunks(GetIndexOfChunkData(x, z)).artificialLightLocationsRedirect)
                LoadedChunks(GetIndexOfChunkData(x, z)).redirectTarget.Add(CopyFaceForRedirect(LoadedChunks(GetIndexOfChunkData(x, z)).up))
                LoadedChunks(GetIndexOfChunkData(x, z)).redirectTarget.Add(CopyFaceForRedirect(LoadedChunks(GetIndexOfChunkData(x, z)).down))
                LoadedChunks(GetIndexOfChunkData(x, z)).redirectTarget.Add(CopyFaceForRedirect(LoadedChunks(GetIndexOfChunkData(x, z)).north))
                LoadedChunks(GetIndexOfChunkData(x, z)).redirectTarget.Add(CopyFaceForRedirect(LoadedChunks(GetIndexOfChunkData(x, z)).east))
                LoadedChunks(GetIndexOfChunkData(x, z)).redirectTarget.Add(CopyFaceForRedirect(LoadedChunks(GetIndexOfChunkData(x, z)).south))
                LoadedChunks(GetIndexOfChunkData(x, z)).redirectTarget.Add(CopyFaceForRedirect(LoadedChunks(GetIndexOfChunkData(x, z)).west))
            End If
        End If
    End Sub

    Private Shared Function CopyFaceForRedirect(original As ChunkFace) As ChunkFace
        Dim copy As New ChunkFace
        ReDim copy.blockType(original.blockType.Length - 1)
        Array.Copy(original.blockType, copy.blockType, original.blockType.Length)
        ReDim copy.blockTypeData(original.blockType.Length - 1)
        Array.Copy(original.blockTypeData, copy.blockTypeData, original.blockTypeData.Length)
        ReDim copy.coords(original.coords.Length - 1)
        Array.Copy(original.coords, copy.coords, original.coords.Length)
        ReDim copy.lightArtificial(original.lightArtificial.Length - 1)
        Array.Copy(original.lightArtificial, copy.lightArtificial, original.lightArtificial.Length)
        ReDim copy.lightFinal(original.lightFinal.Length - 1)
        Array.Copy(original.lightFinal, copy.lightFinal, original.lightFinal.Length)
        ReDim copy.lightNatural(original.lightNatural.Length - 1)
        Array.Copy(original.lightNatural, copy.lightNatural, original.lightNatural.Length)
        Return copy
    End Function

    Private Shared Sub BorderChunkReload()
        Dim offsetX As Integer = Player.chunkX - RENDERDISTANCE
        Dim offsetZ As Integer = Player.chunkZ - RENDERDISTANCE

        For i = 1 To RENDERDISTANCE * 2 - 1
            For j = 1 To RENDERDISTANCE * 2 - 1
                If i = 1 Or j = 1 Or i = RENDERDISTANCE * 2 - 1 Or j = RENDERDISTANCE * 2 - 1 Then
                    If ChunksDataIndex(i, j) <> -1 Then
                        LoadedChunks(ChunksDataIndex(i, j)).isLitArtificial = False
                        LoadedChunks(ChunksDataIndex(i, j)).isLitNatural = False
                    End If
                End If
            Next
        Next
    End Sub

    Private Shared Sub ChunkIndexReload()
        Dim offsetX As Integer = Player.chunkX - RENDERDISTANCE
        Dim offsetZ As Integer = Player.chunkZ - RENDERDISTANCE

        For i = 0 To LoadedChunks.Length - 1
            LoadedChunks(i).inUse = False
        Next

        For i = 1 To RENDERDISTANCE * 2 - 1
            For j = 1 To RENDERDISTANCE * 2 - 1
                ChunksDataIndex(i, j) = -1
            Next
        Next

        For i = 0 To LoadedChunks.Length - 1
            If LoadedChunks(i).hasFaces Then
                If ValidChunkDisplay(LoadedChunks(i).x, LoadedChunks(i).z) Then
                    ChunksDataIndex(LoadedChunks(i).x - offsetX, LoadedChunks(i).z - offsetZ) = i
                    LoadedChunks(i).inUse = True
                End If
            End If
        Next

        For i = 1 To RENDERDISTANCE * 2 - 1
            For j = 1 To RENDERDISTANCE * 2 - 1
                If ChunksDataIndex(i, j) = -1 And Not DuplicateCoordsToGenerate(New ChunkCoord(i + offsetX, j + offsetZ), GenerateFaceList) Then
                    GenerateFaceList.Add(New ChunkCoord(i + offsetX, j + offsetZ))
                End If
            Next
        Next

    End Sub

    Public Shared Function DuplicateCoordsToGenerate(ByVal coords As ChunkCoord, ByRef listToCheck As List(Of ChunkCoord)) As Boolean
        For i = 0 To listToCheck.Count - 1
            If listToCheck(i).x = coords.x And listToCheck(i).z = coords.z Then Return True
        Next
        Return False
    End Function

    Public Shared Function ValidChunkDisplay(x As Integer, z As Integer) As Boolean
        If x >= Player.chunkX + RENDERDISTANCE Then Return False
        If x <= Player.chunkX - RENDERDISTANCE Then Return False
        If z >= Player.chunkZ + RENDERDISTANCE Then Return False
        If z <= Player.chunkZ - RENDERDISTANCE Then Return False
        Return True
    End Function

    Public Shared Function GetIndexOfChunkData(x As Integer, z As Integer) As Integer
        For i = 0 To LoadedChunks.Length - 1
            If LoadedChunks(i).hasData Then
                If LoadedChunks(i).x = x And LoadedChunks(i).z = z Then
                    Return i
                End If
            End If
        Next
        Return -1
    End Function

    Private Shared Sub SetBlockLighting(chunkX As Integer, chunkZ As Integer, additionalLight As Boolean, relCoord As Integer)
        Dim chunkIndex As Integer
        LoadedChunks(GetIndexOfChunkData(chunkX, chunkZ)).isLitArtificial = False
        LoadedChunks(GetIndexOfChunkData(chunkX, chunkZ)).isLitNatural = False
        For i = -1 To 1
            For j = -1 To 1
                chunkIndex = GetIndexOfChunkData(chunkX + i, chunkZ + j)
                If chunkIndex > -1 Then
                    If Not DuplicateCoordsToGenerate(New ChunkCoord(chunkX + i, chunkZ + j), GenerateFaceList) Then
                        GenerateFaceList.Add(New ChunkCoord(chunkX + i, chunkZ + j))
                        If Not additionalLight Then ' Clear light
                            For k = 0 To 15
                                Array.Clear(LoadedChunks(chunkIndex).lightArtificial, 0, 65536)
                                LoadedChunks(chunkIndex).isLitArtificial = False
                                LoadedChunks(chunkIndex).toSpreadArtificial(k).Clear()
                                Array.Clear(LoadedChunks(chunkIndex).lightNatural, 0, 65536)
                                LoadedChunks(chunkIndex).isLitNatural = False
                                LoadedChunks(chunkIndex).toSpreadNatural(k).Clear()
                            Next
                        End If
                    End If
                End If
            Next
        Next
        chunkIndex = GetIndexOfChunkData(chunkX, chunkZ)
        If additionalLight Then
            LoadedChunks(chunkIndex).toSpreadArtificial(14).Add(New LightingClass.ToSpread(0, 0, relCoord))
        End If
        LoadedChunks(chunkIndex).additionalLight = additionalLight
    End Sub

    Public Shared Function SetBlock(x As Integer, y As Integer, z As Integer, blockID As Byte, ByRef allChunkData As FEN.ChunkChanges(), furnaceData As Furnace(), Optional orientation As CustomBlocks.TorchBlockAttached = 0, Optional overrideFurnace As Boolean = False) As Byte
        Dim chunkLoc As Integer
        Dim chunkX, chunkZ As Integer
        Dim relX, relZ As Integer
        Dim surround(3)() As Byte
        Dim blockDestroyed As Byte
        Dim torchIndex As Integer
        Dim blockChange As New FEN.BlockChange
        Dim i As Integer = 0
        Dim relCoord As Integer
        Dim additionalLight As Boolean = False
        Dim fullLightingRefresh As Boolean = True
        Dim surroundChunkLighting(2, 2)() As Byte
        Dim surroundChunkIndex(2, 2) As Integer

        chunkLoc = GetIndexOfChunkData(CoordsToChunk(x), CoordsToChunk(z))
        chunkX = CoordsToChunk(x)
        chunkZ = CoordsToChunk(z)

        fpsDrop = True ' Drops fps to so that block input lag drops

        relX = CoordsToBlockRelative(x)
        relZ = CoordsToBlockRelative(z)
        relCoord = relX + y * 16 + relZ * 4096
        blockDestroyed = LoadedChunks(chunkLoc).data(relCoord)
        LoadedChunks(chunkLoc).data(relCoord) = blockID

        additionalLight = False
        If blockID = Terrain.Blocks.torch Or blockID = Terrain.Blocks.furnace + 1 Then
            LoadedChunks(chunkLoc).artificialLightLocations.Add(relCoord)
            LoadedChunks(chunkLoc).artificialLightOrientations.Add(orientation)
            If Not DuplicateCoordsToGenerate(New ChunkCoord(LoadedChunks(chunkLoc).x, LoadedChunks(chunkLoc).z), GenerateFaceList) Then
                additionalLight = True
            End If
        End If
        If blockDestroyed = Terrain.Blocks.torch Or blockDestroyed = Terrain.Blocks.furnace + 1 Then
            torchIndex = LoadedChunks(chunkLoc).artificialLightLocations.IndexOf(relCoord)
            LoadedChunks(chunkLoc).artificialLightLocations.RemoveAt(torchIndex)
            LoadedChunks(chunkLoc).artificialLightOrientations.RemoveAt(torchIndex)
        End If
        If Not overrideFurnace Then
            If blockDestroyed = Terrain.Blocks.furnace Or blockDestroyed = Terrain.Blocks.furnace + 1 Then
                For i = 0 To furnaceData.Length - 1
                    If Not IsNothing(furnaceData(i)) Then
                        If furnaceData(i).chunkX = chunkX And furnaceData(i).chunkZ = chunkZ And furnaceData(i).myCoord = relCoord Then
                            furnaceData(i).inUse = False
                        End If
                    End If
                Next
            End If
            If blockID = Terrain.Blocks.furnace Or blockID = Terrain.Blocks.furnace + 1 Then
                While i < furnaceData.Length AndAlso Not IsNothing(furnaceData(i)) AndAlso furnaceData(i).inUse
                    i += 1
                End While
                If i = furnaceData.Length Then
                    ReDim Preserve furnaceData(i * 2)
                End If
                For j = 0 To furnaceData.Length - 1
                    If IsNothing(furnaceData(j)) Then
                        furnaceData(j) = New Furnace()
                        furnaceData(j).inUse = False
                    End If
                Next
                furnaceData(i).inUse = True
                furnaceData(i).chunkX = LoadedChunks(chunkLoc).x
                furnaceData(i).chunkZ = LoadedChunks(chunkLoc).z
                furnaceData(i).myCoord = relCoord
            End If
        End If
        If blockID = Terrain.Blocks.air Then ' water spills

        End If
        If blockID <> Terrain.Blocks.air AndAlso blockID <> Terrain.Blocks.furnace + 1 AndAlso blockID <> Terrain.Blocks.torch Then ' If solid block is placed
            fullLightingRefresh = False
            surroundChunkIndex = GetSurroundIndexFull(New ChunkCoord(chunkX, chunkZ))
            For i = 0 To 2
                For j = 0 To 2
                    surroundChunkLighting(i, j) = LoadedChunks(surroundChunkIndex(i, j)).lightNatural
                Next
            Next
            GeneratePartialLighting(New ChunkCoord(chunkX, chunkZ), relCoord, surroundChunkLighting)
            For i = 0 To 2
                For j = 0 To 2
                    surroundChunkLighting(i, j) = LoadedChunks(surroundChunkIndex(i, j)).lightArtificial
                Next
            Next
            GeneratePartialLighting(New ChunkCoord(chunkX, chunkZ), relCoord, surroundChunkLighting)
            additionalLight = False ' change this for optimisation later
        End If
        'LoadedChunks(chunkLoc).hasFaces = False
        GenerateFaceList.Add(New ChunkCoord(chunkX, chunkZ))
        SetBlockLighting(chunkX, chunkZ, additionalLight, relCoord)
        blockChange.blockID = blockID
        blockChange.coord = relX + y * 16 + relZ * 4096
        FEN.EditBlock(blockChange, FEN.GetChangesFromChunk(New ChunkCoord(CoordsToChunk(x), CoordsToChunk(z)), allChunkData, True))
        If Not DuplicateCoordsToGenerate(New ChunkCoord(chunkX, chunkZ), GenerateSourceList) Then
            GenerateSourceList.Add(New ChunkCoord(chunkX, chunkZ))
        End If
        Return blockDestroyed
    End Function

    Private Shared Sub GeneratePartialLighting(sourceChunk As ChunkCoord, sourceBlock As Integer, ByRef surroundData(,)() As Byte)
        Dim toSpreadBlock As New List(Of Integer)
        Dim toSpreadChunk As New List(Of ChunkCoord)
        Dim toSpreadBlockSource As Integer
        Dim toSpreadBlockDest As Integer
        Dim toSpreadChunkSource As ChunkCoord
        Dim toSpreadChunkDest As ChunkCoord
        Dim lightLevel As Byte
        Dim lightLevelDest As Byte
        Dim outOfRange As Boolean

        For i = 0 To 2
            For j = 0 To 2
                BlocksToRelightBlocks(i, j) = New List(Of Integer)
            Next
        Next

        toSpreadBlock.Add(sourceBlock)
        toSpreadChunk.Add(sourceChunk)

        While toSpreadBlock.Count > 0
            toSpreadBlockSource = toSpreadBlock(0)
            toSpreadChunkSource = toSpreadChunk(0)
            lightLevel = surroundData(toSpreadChunkSource.x - sourceChunk.x + 1, toSpreadChunkSource.z - sourceChunk.z + 1)(toSpreadBlockSource)
            For i = 0 To 5
                outOfRange = False
                toSpreadChunkDest = New ChunkCoord(toSpreadChunkSource.x, toSpreadChunkSource.z)
                Select Case i
                    Case 0
                        If (toSpreadBlockSource And &HF) = 0 Then
                            toSpreadBlockDest = toSpreadBlockSource + 15
                            toSpreadChunkDest = New ChunkCoord(toSpreadChunkSource.x - 1, toSpreadChunkSource.z)
                        Else
                            toSpreadBlockDest = toSpreadBlockSource - 1
                        End If
                    Case 1
                        If (toSpreadBlockSource And &HF) = 15 Then
                            toSpreadBlockDest = toSpreadBlockSource - 15
                            toSpreadChunkDest = New ChunkCoord(toSpreadChunkSource.x + 1, toSpreadChunkSource.z)
                        Else
                            toSpreadBlockDest = toSpreadBlockSource + 1
                        End If
                    Case 2
                        If (toSpreadBlockSource And &HF000) = 0 Then
                            toSpreadBlockDest = toSpreadBlockSource + 15 * 4096
                            toSpreadChunkDest = New ChunkCoord(toSpreadChunkSource.x, toSpreadChunkSource.z - 1)
                        Else
                            toSpreadBlockDest = toSpreadBlockSource - 4096
                        End If
                    Case 3
                        If (toSpreadBlockSource And &HF000) = 15 * 4096 Then
                            toSpreadBlockDest = toSpreadBlockSource - 15 * 4096
                            toSpreadChunkDest = New ChunkCoord(toSpreadChunkSource.x, toSpreadChunkSource.z + 1)
                        Else
                            toSpreadBlockDest = toSpreadBlockSource + 4096
                        End If
                    Case 4
                        If (toSpreadBlockSource And &HFF0) = 0 Then
                            outOfRange = True
                        Else
                            toSpreadBlockDest = toSpreadBlockSource - 16
                        End If
                    Case 5
                        If (toSpreadBlockSource And &HFF0) = 255 * 16 Then
                            outOfRange = True
                        Else
                            toSpreadBlockDest = toSpreadBlockSource + 16
                        End If
                End Select
                If toSpreadChunkDest.x - sourceChunk.x + 1 > 2 OrElse toSpreadChunkDest.x - sourceChunk.x + 1 < 0 Then
                    outOfRange = True
                End If
                If toSpreadChunkDest.z - sourceChunk.z + 1 > 2 OrElse toSpreadChunkDest.z - sourceChunk.z + 1 < 0 Then
                    outOfRange = True
                End If
                If Not outOfRange Then
                    lightLevelDest = surroundData(toSpreadChunkDest.x - sourceChunk.x + 1, toSpreadChunkDest.z - sourceChunk.z + 1)(toSpreadBlockDest)
                    If (lightLevelDest < lightLevel Or (lightLevelDest = 15 And i = 4 And lightLevel = 15)) AndAlso Not BlocksToRelightBlocks(toSpreadChunkDest.x - sourceChunk.x + 1, toSpreadChunkDest.z - sourceChunk.z + 1).Contains(toSpreadBlockDest) Then
                        toSpreadBlock.Add(toSpreadBlockDest)
                        toSpreadChunk.Add(toSpreadChunkDest)
                    End If
                End If
            Next
            surroundData(toSpreadChunkSource.x - sourceChunk.x + 1, toSpreadChunkSource.z - sourceChunk.z + 1)(toSpreadBlockSource) = 0
            toSpreadBlock.RemoveAt(0)
            toSpreadChunk.RemoveAt(0)
            BlocksToRelightBlocks(toSpreadChunkSource.x - sourceChunk.x + 1, toSpreadChunkSource.z - sourceChunk.z + 1).Add(toSpreadBlockSource)
        End While

        BlocksToRelightChunks = sourceChunk
        BlocksToRelightDepthArtificial = 15
        BlocksToRelightDepthNatural = 15

    End Sub

    Public Shared Sub LoadTextures(names As ImportedData.BlockData(), numTextures As Integer, ByRef itemNames As ImportedData.ItemData(), numTexturesItem As Integer, ByRef openGlData As OpenGL.OpenGlData)
        Dim tex As New Bitmap
        Dim GUI As String() = {"Hotbar", "HotbarSelected", "Slot", "Cursor", "Selected", "FurnaceOff", "FurnaceOn", "ArrowEmptyA", "ArrowEmptyB", "ArrowFillA", "ArrowFillB", "HeartEmpty", "HeartHalf", "HeartFull", "Bar"}
        Dim texPtr As IntPtr = Marshal.AllocHGlobal(1000000)
        Dim filePath As String
        Dim runningCount As Integer = 0
        For i = 0 To names.Length - 1
            For j = 0 To names(i).UniqueFaces.Length - 1
                filePath = "Resource\" & names(i).Name & "\" & names(i).UniqueFaces(j)
                If IO.File.Exists(filePath & ".bmp") Then
                    tex.ReadData(filePath, i = Terrain.Blocks.water)
                Else
                    tex.ReadData("Resource\Demo", False)
                    WriteLine("Error loading " & filePath)
                End If
                For k = 0 To 1023
                    Marshal.WriteByte(texPtr + k + (runningCount) * 1024, Marshal.ReadByte(tex.texture + k))
                Next
                runningCount += 1
            Next
        Next
        NumTexturesBlocks = runningCount
        For i = 0 To itemNames.Length - 1
            filePath = "Resource\Items\" & itemNames(i).Name
            If IO.File.Exists(filePath & ".bmp") Then
                tex.ReadData(filePath, False)
                WriteLine("Loaded " & filePath)
            Else
                tex.ReadData("Resource\Demo", False)
                WriteLine("Error loading " & filePath)
            End If
            For k = 0 To 1023
                Marshal.WriteByte(texPtr + k + (runningCount) * 1024, Marshal.ReadByte(tex.texture + k))
            Next
            itemNames(i).TextureIndex = runningCount
            runningCount += 1
        Next
        numTexturesItem = runningCount - NumTexturesBlocks
        For i = 0 To 19 'Load cracks and numbers
            filePath = "Resource\" & If(i < 10, "Destroy\", "Numbers\") & i Mod 10
            If IO.File.Exists(filePath & ".bmp") Then
                tex.ReadData(filePath, False)
                WriteLine("Loaded " & filePath)
            Else
                tex.ReadData("Resource\Demo", False)
                WriteLine("Error loading " & filePath)
            End If
            For k = 0 To 1023
                Marshal.WriteByte(texPtr + k + (runningCount) * 1024, Marshal.ReadByte(tex.texture + k))
            Next
            runningCount += 1
        Next
        For i = 0 To 25 'Load letters
            filePath = "Resource\Letters\" & ChrW(i + AscW("A"c))
            If IO.File.Exists(filePath & ".bmp") Then
                tex.ReadData(filePath, False)
                WriteLine("Loaded " & filePath)
            Else
                tex.ReadData("Resource\Demo", False)
                WriteLine("Error loading " & filePath)
            End If
            For k = 0 To 1023
                Marshal.WriteByte(texPtr + k + (runningCount) * 1024, Marshal.ReadByte(tex.texture + k))
            Next
            runningCount += 1
        Next
        For i = 0 To GUI.Length - 1
            If IO.File.Exists("Resource\GUI\" & GUI(i) & ".bmp") Then
                tex.ReadData("Resource\GUI\" & GUI(i), False)
                WriteLine("Resource\GUI\" & GUI(i))
            Else
                tex.ReadData("Resource\Demo", False)
                WriteLine("Error loading resource")
            End If
            For k = 0 To 1023
                Marshal.WriteByte(texPtr + k + (runningCount) * 1024, Marshal.ReadByte(tex.texture + k))
            Next
            runningCount += 1
        Next
        ZombieTextureStart = runningCount 'NumTexturesBlocks * 3 + 20 + GUI.Length + NumTexturesItems
        NumTexturesTotal = ZombieTextureStart + LoadZombieTextures(runningCount, tex, texPtr)
        OpenGL.Initialise(texPtr, NumTexturesTotal, openGlData)
    End Sub

    Private Shared Function LoadZombieTextures(ByRef runningCount As Integer, ByRef tex As Bitmap, ByVal texPtr As IntPtr) As Integer
        Dim numTextures As Integer = 0
        Dim bodyParts As String() = IO.File.ReadAllLines("Resource\Zombie\Model.txt")
        Dim faces As String = "BDLFUR"
        For i = 0 To bodyParts.Length - 1
            For j = 0 To faces.Length - 1
                If IO.File.Exists("Resource\Zombie\" & bodyParts(i) & faces(j) & ".bmp") Then
                    tex.ReadData("Resource\Zombie\" & bodyParts(i) & faces(j), False)
                    WriteLine("Resource\Zombie\" & bodyParts(i) & faces(j))
                Else
                    tex.ReadData("Resource\Demo", False)
                    WriteLine("Error loading resource")
                End If
                For k = 0 To 1023
                    Marshal.WriteByte(texPtr + k + (runningCount + numTextures) * 1024, Marshal.ReadByte(tex.texture + k))
                Next
                numTextures += 1
            Next
        Next
        Return numTextures
    End Function

    Structure ChunkFaces
        Dim x As Integer
        Dim z As Integer
        Dim hasData As Boolean
        Dim hasFullData As Boolean
        Dim hasFaces As Boolean
        Dim isLitNatural As Boolean
        Dim isLitArtificial As Boolean
        Dim overflowNatural As Boolean
        Dim overflowArtificial As Boolean
        Dim refreshArtificial As Boolean
        Dim refreshNatural As Boolean
        Dim inUse As Boolean
        Dim up As ChunkFace
        Dim down As ChunkFace
        Dim north As ChunkFace
        Dim south As ChunkFace
        Dim west As ChunkFace
        Dim east As ChunkFace
        Dim upWater As ChunkFace
        Dim downWater As ChunkFace
        Dim northWater As ChunkFace
        Dim southWater As ChunkFace
        Dim westWater As ChunkFace
        Dim eastWater As ChunkFace
        Dim data As Byte()
        Dim lightNatural As Byte()
        Dim lightArtificial As Byte()
        Dim lightNaturalSmooth As Byte()
        Dim lightArtificialSmooth As Byte()
        Dim treeLocs As Terrain.TreeLocation()
        Dim artificialLightLocations As List(Of Integer)
        Dim artificialLightLocationsRedirect As List(Of Integer)
        Dim toSpreadNatural As List(Of LightingClass.ToSpread)()
        Dim toSpreadArtificial As List(Of LightingClass.ToSpread)()
        Dim artificialLightOrientations As List(Of CustomBlocks.TorchBlockAttached)
        Dim artificialLightOrientationsRedirect As List(Of CustomBlocks.TorchBlockAttached)
        Dim overflowArtificialSaved As List(Of LightingClass.ToSpread)()
        Dim overflowNaturalSaved As List(Of LightingClass.ToSpread)()
        Dim placed As Boolean
        Dim redirectTarget As List(Of ChunkFace)
        Dim redirect As Boolean
        Dim isSmooth As Boolean
        Dim additionalLight As Boolean
    End Structure

    Structure ChunkFace
        Dim coords As Single()
        Dim blockType As Byte()
        Dim blockTypeData As Single()
        Dim lightNatural As Byte()
        Dim lightFinal As Byte()
        Dim lightArtificial As Byte()
        Sub New(init As Boolean)
            coords = {}
            blockType = {}
            blockTypeData = {}
            lightNatural = {}
            lightFinal = {}
            lightArtificial = {}
        End Sub
    End Structure


    Public Shared Function GetFacesOfBlock(xRel As Single, yRel As Integer, zRel As Single, chunkX As Integer, chunkZ As Integer) As Single()
        Dim returnValue As New List(Of Single)
        Dim x, y, z As Integer
        Dim dimension1 As Single() = {0, 0, 1, 1}
        Dim dimension2 As Single() = {0, 1, 1, 0}

        x = CInt(Math.Floor(xRel)) + 16 * chunkX
        y = yRel
        z = CInt(Math.Floor(zRel)) + 16 * chunkZ

        For j = 0 To 1
            For i = 0 To 3
                returnValue.Add(dimension1(i) + x)
                returnValue.Add(dimension2(i) + y)
                returnValue.Add(z + j)
                returnValue.Add(1)
            Next
            For i = 0 To 3
                returnValue.Add(x + j)
                returnValue.Add(dimension2(i) + y)
                returnValue.Add(dimension1(i) + z)
                returnValue.Add(1)
            Next
            For i = 0 To 3
                returnValue.Add(dimension1(i) + x)
                returnValue.Add(y + j)
                returnValue.Add(dimension2(i) + z)
                returnValue.Add(1)
            Next
        Next
        Return returnValue.ToArray()
    End Function

    Public Shared Function GetFacesOfBlock(xRel As Single, yRel As Integer, zRel As Single, chunkX As Integer, chunkZ As Integer, face As RayTracing.Faces) As Single()
        Dim returnValue As New List(Of Single)
        Dim x, y, z As Integer
        Dim dimension1 As Single() = {0, 0, 1, 1}
        Dim dimension2 As Single() = {0, 1, 1, 0}

        x = CInt(Math.Floor(xRel)) + 16 * chunkX
        y = yRel
        z = CInt(Math.Floor(zRel)) + 16 * chunkZ

        If face = RayTracing.Faces.forward Or face = RayTracing.Faces.backwards Then
            For i = 0 To 3
                returnValue.Add(dimension1(i) + x)
                returnValue.Add(dimension2(i) + y)
                If Player.z + Player.chunkZ * 16 > z Then
                    returnValue.Add(z + 1)
                Else
                    returnValue.Add(z)
                End If
                returnValue.Add(1)
            Next
        End If
        If face = RayTracing.Faces.right Or face = RayTracing.Faces.left Then
            For i = 0 To 3
                If Player.x + Player.chunkX * 16 > x Then
                    returnValue.Add(x + 1)
                Else
                    returnValue.Add(x)
                End If
                returnValue.Add(dimension2(i) + y)
                returnValue.Add(dimension1(i) + z)
                returnValue.Add(1)
            Next
        End If
        If face = RayTracing.Faces.up Or face = RayTracing.Faces.down Then
            For i = 0 To 3
                returnValue.Add(dimension1(i) + x)
                If Player.y > y Then
                    returnValue.Add(y + 1)
                Else
                    returnValue.Add(y)
                End If
                returnValue.Add(dimension2(i) + z)
                returnValue.Add(1)
            Next
        End If

        Return returnValue.ToArray()
    End Function

    Private Shared Sub EnsureIsSomething(ByRef chunkFaces As ChunkFaces)
        If IsNothing(chunkFaces.lightArtificialSmooth) OrElse chunkFaces.lightArtificialSmooth.Length < 65536 Then
            ReDim chunkFaces.lightArtificialSmooth(chunkFaces.lightArtificial.Length - 1)
        End If
        If IsNothing(chunkFaces.lightNaturalSmooth) OrElse chunkFaces.lightNaturalSmooth.Length < 65536 Then
            ReDim chunkFaces.lightNaturalSmooth(chunkFaces.lightNatural.Length - 1)
        End If
        If IsNothing(chunkFaces.data) OrElse chunkFaces.data.Length < 65536 Then
            ReDim chunkFaces.data(chunkFaces.data.Length - 1)
        End If
    End Sub

    Public Shared Sub SmoothLighting(ByRef chunkFaces As ChunkFaces, ByRef surroundSouth As ChunkFaces, ByRef surroundWest As ChunkFaces, ByRef surroundSW As ChunkFaces, blockData As ImportedData.BlockData())
        Dim solid, lightA, lightN As Integer
        Dim blockLocation As Integer
        EnsureIsSomething(chunkFaces)
        EnsureIsSomething(surroundSouth)
        EnsureIsSomething(surroundSW)
        EnsureIsSomething(surroundWest)
        For x = 0 To 15
            For z = 0 To 15
                For y = 0 To 255
                    solid = 0
                    lightA = 0
                    lightN = 0
                    For xg = -1 To 0
                        For yg = -1 To 0
                            For zg = -1 To 0
                                blockLocation = x + xg + (z + zg) * 4096 + (y + yg) * 16
                                If xg + x >= 0 AndAlso z + zg >= 0 AndAlso y + yg >= 0 Then
                                    If chunkFaces.data(blockLocation) = Terrain.Blocks.air OrElse chunkFaces.data(blockLocation) = Terrain.Blocks.torch OrElse chunkFaces.data(blockLocation) = Terrain.Blocks.furnace + 1 OrElse chunkFaces.data(blockLocation) = Terrain.Blocks.water Then
                                        lightA += chunkFaces.lightArtificial(blockLocation)
                                        lightN += chunkFaces.lightNatural(blockLocation)
                                    Else
                                        solid += 1
                                    End If
                                ElseIf y + yg < 0 Then
                                    solid += 1
                                ElseIf x + xg >= 0 AndAlso z + zg < 0 Then
                                    blockLocation += 65536
                                    If surroundSouth.data(blockLocation) = Terrain.Blocks.air OrElse surroundSouth.data(blockLocation) = Terrain.Blocks.torch OrElse surroundSouth.data(blockLocation) = Terrain.Blocks.furnace + 1 OrElse surroundSouth.data(blockLocation) = Terrain.Blocks.water Then
                                        lightA += surroundSouth.lightArtificial(blockLocation)
                                        lightN += surroundSouth.lightNatural(blockLocation)
                                    Else
                                        solid += 1
                                    End If
                                    blockLocation -= 65536
                                ElseIf x + xg < 0 AndAlso z + zg >= 0 Then
                                    blockLocation += 16
                                    If surroundWest.data(blockLocation) = Terrain.Blocks.air OrElse surroundWest.data(blockLocation) = Terrain.Blocks.torch OrElse surroundWest.data(blockLocation) = Terrain.Blocks.furnace + 1 OrElse surroundWest.data(blockLocation) = Terrain.Blocks.water Then
                                        lightA += surroundWest.lightArtificial(blockLocation)
                                        lightN += surroundWest.lightNatural(blockLocation)
                                    Else
                                        solid += 1
                                    End If
                                    blockLocation -= 16
                                Else
                                    blockLocation += (16 + 65536)
                                    If surroundSW.data(blockLocation) = Terrain.Blocks.air OrElse surroundSW.data(blockLocation) = Terrain.Blocks.torch OrElse surroundSW.data(blockLocation) = Terrain.Blocks.furnace + 1 OrElse surroundSW.data(blockLocation) = Terrain.Blocks.water Then
                                        lightA += surroundSW.lightArtificial(blockLocation)
                                        lightN += surroundSW.lightNatural(blockLocation)
                                    Else
                                        solid += 1
                                    End If
                                    blockLocation -= (16 + 65536)
                                End If
                            Next
                        Next
                    Next
                    If solid < 8 Then
                        lightA = CInt((lightA - solid) / (8 - solid))
                        lightN = CInt((lightN - solid) / (8 - solid))
                    End If
                    If lightA > 0 Then
                        lightA = lightA
                    End If
                    If lightA < 0 Then lightA = 0
                    If lightN < 0 Then lightN = 0
                    blockLocation = x + y * 16 + z * 4096
                    chunkFaces.lightNaturalSmooth(blockLocation) = CByte(lightN)
                    chunkFaces.lightArtificialSmooth(blockLocation) = CByte(lightA)
                Next
            Next
        Next
    End Sub

    Public Shared Sub GetFaces(ByRef allChunkFaces(,) As ChunkFaces, blockData As ImportedData.BlockData(), water As Boolean)
        Dim coordsX As New List(Of Single)
        Dim coordsY As New List(Of Single)
        Dim coordsZ As New List(Of Single)

        Dim listFacesX As New List(Of Byte)
        Dim listFacesY As New List(Of Byte)
        Dim listFacesZ As New List(Of Byte)

        Dim listColoursX As New List(Of Byte)
        Dim listColoursY As New List(Of Byte)
        Dim listColoursZ As New List(Of Byte)

        Dim listColoursXA As New List(Of Byte)
        Dim listColoursYA As New List(Of Byte)
        Dim listColoursZA As New List(Of Byte)

        Dim dimension1 As Single() = {0, 0, 1, 1}
        Dim dimension2 As Single() = {0, 1, 1, 0}
        Dim xG, yG, zG As Integer
        Dim x, y, z As Integer
        Dim currentBlock As Byte
        Dim currentBlockLoc As Integer
        Dim i As Integer
        Dim nextBlock As Byte
        Dim nearLight As Byte
        Dim nearLightA As Byte
        Dim lightLevel As Byte
        Dim tempTarget As Integer = -1
        Dim lightIndexSmooth As Integer

        Dim chunkFaces As ChunkFaces = allChunkFaces(1, 1)

        Dim lightRedirectA As Byte()

        Dim xIndexCheck As Integer() = {1, 1, 2, 2}
        Dim zIndexCheck As Integer() = {1, 2, 1, 2}

        Dim fileOut As New Text.StringBuilder
        Dim fout2 As New Text.StringBuilder
        Dim chunkPositionX As Integer
        Dim chunkPositionZ As Integer

        If SMOOTHLIGHTINGENABLE Then
            For i = 0 To 3
                If Not allChunkFaces(xIndexCheck(i), zIndexCheck(i)).isSmooth Then
                    SmoothLighting(allChunkFaces(xIndexCheck(i), zIndexCheck(i)), allChunkFaces(xIndexCheck(i), zIndexCheck(i) - 1), allChunkFaces(xIndexCheck(i) - 1, zIndexCheck(i)), allChunkFaces(xIndexCheck(i) - 1, zIndexCheck(i) - 1), blockData)
                    allChunkFaces(xIndexCheck(i), zIndexCheck(i)).isSmooth = True
                End If
            Next
        End If
        chunkFaces = allChunkFaces(1, 1)

        If SMOOTHLIGHTINGENABLE Then
            lightRedirectA = chunkFaces.lightArtificialSmooth
        Else
            lightRedirectA = chunkFaces.lightArtificial
        End If

        For i = 0 To chunkFaces.lightArtificial.Length - 17
            If chunkFaces.lightArtificial(i + 16) > 0 And chunkFaces.data(i) > 0 Then
                tempTarget = i
                Exit For
            End If
        Next

        zG = 4096
        xG = 1
        yG = 16
        For j = 0 To 1
            coordsX.Clear()
            coordsY.Clear()
            coordsZ.Clear()
            listFacesX.Clear()
            listFacesY.Clear()
            listFacesZ.Clear()
            listColoursX.Clear()
            listColoursY.Clear()
            listColoursZ.Clear()
            listColoursXA.Clear()
            listColoursYA.Clear()
            listColoursZA.Clear()

            For currentBlockLoc = 0 To 65535
                x = currentBlockLoc Mod &H10
                y = (currentBlockLoc And &HFF0) >> 4
                z = currentBlockLoc >> 12
                currentBlock = chunkFaces.data(currentBlockLoc)
                If currentBlock > 0 AndAlso currentBlock <> Terrain.Blocks.torch Then
                    If x + xG < 0 Then
                        nextBlock = allChunkFaces(0, 1).data(currentBlockLoc + xG + 16)
                        nearLight = allChunkFaces(0, 1).lightNatural(currentBlockLoc + xG + 16)
                        nearLightA = allChunkFaces(0, 1).lightArtificial(currentBlockLoc + xG + 16)
                    ElseIf x + xG > 15 Then
                        nextBlock = allChunkFaces(2, 1).data(currentBlockLoc + xG - 16)
                        nearLight = allChunkFaces(2, 1).lightNatural(currentBlockLoc + xG - 16)
                        nearLightA = allChunkFaces(2, 1).lightArtificial(currentBlockLoc + xG - 16)
                    Else
                        nextBlock = chunkFaces.data(currentBlockLoc + xG)
                        nearLight = chunkFaces.lightNatural(currentBlockLoc + xG)
                        nearLightA = chunkFaces.lightArtificial(currentBlockLoc + xG)
                    End If
                    If water = (Terrain.Blocks.water = currentBlock) AndAlso (nextBlock = 0 OrElse nextBlock = Terrain.Blocks.torch OrElse (nextBlock = Terrain.Blocks.water And currentBlock <> Terrain.Blocks.water And Not water)) Then
                        For i = 0 To 3
                            If xG > 0 Then
                                coordsX.Add(x + 1)
                            Else
                                coordsX.Add(x)
                            End If
                            coordsX.Add(dimension2(i) + y)
                            coordsX.Add(dimension1(i) + z)
                            coordsX.Add(1)
                        Next
                        If xG > 0 Then
                            listFacesX.Add(blockData(currentBlock).FacesIndex(2))
                        Else
                            listFacesX.Add(blockData(currentBlock).FacesIndex(3))
                        End If
                        lightLevel = nearLight
                        For k = 0 To 3
                            If SMOOTHLIGHTINGENABLE Then
                                If xG > 0 Then
                                    lightIndexSmooth = CInt(currentBlockLoc + dimension1(k) * 4096 + dimension2(k) * 16 + 1)
                                Else
                                    lightIndexSmooth = CInt(currentBlockLoc + dimension1(k) * 4096 + dimension2(k) * 16)
                                End If
                                chunkPositionX = 1
                                chunkPositionZ = 1
                                If x + xG > 15 Then
                                    chunkPositionX = 2
                                    lightIndexSmooth -= 16
                                End If
                                If z + dimension1(k) > 15 Then
                                    chunkPositionZ = 2
                                    lightIndexSmooth -= 65536
                                End If
                                lightLevel = allChunkFaces(chunkPositionX, chunkPositionZ).lightNaturalSmooth(lightIndexSmooth)
                                nearLightA = allChunkFaces(chunkPositionX, chunkPositionZ).lightArtificialSmooth(lightIndexSmooth)
                            End If
                            For i = 0 To 2
                                listColoursX.Add(lightLevel)
                                listColoursXA.Add(nearLightA)
                            Next
                        Next
                    End If
                    If Not (y * 16 + yG < 0 OrElse y * 16 + yG > 4095) Then
                        nextBlock = chunkFaces.data(currentBlockLoc + yG)
                        If (nextBlock = 0 OrElse nextBlock = Terrain.Blocks.torch OrElse (nextBlock = Terrain.Blocks.water And currentBlock <> Terrain.Blocks.water And Not water)) AndAlso water = (Terrain.Blocks.water = currentBlock) Then
                            nearLight = chunkFaces.lightNatural(currentBlockLoc + yG)
                            nearLightA = chunkFaces.lightArtificial(currentBlockLoc + yG)
                            For i = 0 To 3
                                coordsY.Add(dimension1(i) + x)
                                If yG > 0 Then
                                    coordsY.Add(y + 1)
                                Else
                                    coordsY.Add(y)
                                End If
                                coordsY.Add(dimension2(i) + z)
                                coordsY.Add(1)
                            Next
                            If yG > 0 Then
                                listFacesY.Add(CByte(blockData(currentBlock).FacesIndex(0)))
                            Else
                                listFacesY.Add(CByte(blockData(currentBlock).FacesIndex(1)))
                            End If
                            lightLevel = nearLight
                            For k = 0 To 3
                                If SMOOTHLIGHTINGENABLE Then
                                    If yG > 0 Then
                                        lightIndexSmooth = CInt(currentBlockLoc + dimension1(k) + dimension2(k) * 4096 + 16)
                                    Else
                                        lightIndexSmooth = CInt(currentBlockLoc + dimension1(k) + dimension2(k) * 4096)
                                    End If
                                    chunkPositionX = 1
                                    chunkPositionZ = 1
                                    If x + dimension1(k) > 15 Then
                                        lightIndexSmooth -= 16
                                        chunkPositionX = 2
                                    End If
                                    If z + dimension2(k) > 15 Then
                                        lightIndexSmooth -= 65536
                                        chunkPositionZ = 2
                                    End If

                                    lightLevel = allChunkFaces(chunkPositionX, chunkPositionZ).lightNaturalSmooth(lightIndexSmooth)
                                    nearLightA = allChunkFaces(chunkPositionX, chunkPositionZ).lightArtificialSmooth(lightIndexSmooth)
                                End If
                                For i = 0 To 2
                                    listColoursY.Add(lightLevel)
                                    listColoursYA.Add(nearLightA)
                                Next
                            Next
                        End If
                    End If
                    If z * 4096 + zG < 0 Then
                        nextBlock = allChunkFaces(1, 0).data(currentBlockLoc + zG + 65536)
                        nearLight = allChunkFaces(1, 0).lightNatural(currentBlockLoc + zG + 65536)
                        nearLightA = allChunkFaces(1, 0).lightArtificial(currentBlockLoc + zG + 65536)
                    ElseIf z * 4096 + zG > 65535 Then
                        nextBlock = allChunkFaces(1, 2).data(currentBlockLoc + zG - 65536)
                        nearLight = allChunkFaces(1, 2).lightNatural(currentBlockLoc + zG - 65536)
                        nearLightA = allChunkFaces(1, 2).lightArtificial(currentBlockLoc + zG - 65536)
                    Else
                        nextBlock = chunkFaces.data(currentBlockLoc + zG)
                        nearLight = chunkFaces.lightNatural(currentBlockLoc + zG)
                        nearLightA = chunkFaces.lightArtificial(currentBlockLoc + zG)
                    End If
                    If (nextBlock = 0 OrElse nextBlock = Terrain.Blocks.torch OrElse (nextBlock = Terrain.Blocks.water And currentBlock <> Terrain.Blocks.water And Not water)) AndAlso water = (Terrain.Blocks.water = currentBlock) Then
                        For i = 0 To 3
                            coordsZ.Add(dimension1(i) + x)
                            coordsZ.Add(dimension2(i) + y)
                            If zG > 0 Then
                                coordsZ.Add(z + 1)
                            Else
                                coordsZ.Add(z)
                            End If
                            coordsZ.Add(1)
                        Next
                        If zG > 0 Then
                            listFacesZ.Add(CByte(blockData(currentBlock).FacesIndex(4)))
                        Else
                            listFacesZ.Add(CByte(blockData(currentBlock).FacesIndex(5)))
                        End If
                        lightLevel = nearLight
                        For k = 0 To 3
                            If SMOOTHLIGHTINGENABLE Then
                                If zG > 0 Then
                                    lightIndexSmooth = CInt(currentBlockLoc + dimension1(k) + dimension2(k) * 16 + 4096)
                                Else
                                    lightIndexSmooth = CInt(currentBlockLoc + dimension1(k) + dimension2(k) * 16)
                                End If
                                chunkPositionX = 1
                                chunkPositionZ = 1
                                If x + dimension1(k) > 15 Then
                                    lightIndexSmooth -= 16
                                    chunkPositionX = 2
                                End If
                                If z + (zG \ 4096) > 15 Then
                                    lightIndexSmooth -= 65536
                                    chunkPositionZ = 2
                                End If
                                lightLevel = allChunkFaces(chunkPositionX, chunkPositionZ).lightNaturalSmooth(lightIndexSmooth)
                                nearLightA = allChunkFaces(chunkPositionX, chunkPositionZ).lightArtificialSmooth(lightIndexSmooth)
                            End If
                            For i = 0 To 2
                                listColoursZ.Add(lightLevel)
                                listColoursZA.Add(nearLightA)
                            Next
                        Next
                    End If
                End If
            Next
            zG = -4096
            xG = -1
            yG = -16
            If water Then
                If j = 0 Then
                    TransferFaceData(chunkFaces.eastWater, chunkFaces.upWater, chunkFaces.northWater, chunkFaces.x, chunkFaces.z, coordsX, coordsY, coordsZ, listFacesX, listFacesY, listFacesZ, listColoursX, listColoursY, listColoursZ, listColoursXA, listColoursYA, listColoursZA)
                Else
                    TransferFaceData(chunkFaces.westWater, chunkFaces.downWater, chunkFaces.southWater, chunkFaces.x, chunkFaces.z, coordsX, coordsY, coordsZ, listFacesX, listFacesY, listFacesZ, listColoursX, listColoursY, listColoursZ, listColoursXA, listColoursYA, listColoursZA)
                End If
                If listFacesY.Count > 0 Then
                    j = j
                End If
            Else
                If j = 0 Then
                    TransferFaceData(chunkFaces.east, chunkFaces.up, chunkFaces.north, chunkFaces.x, chunkFaces.z, coordsX, coordsY, coordsZ, listFacesX, listFacesY, listFacesZ, listColoursX, listColoursY, listColoursZ, listColoursXA, listColoursYA, listColoursZA)
                Else
                    TransferFaceData(chunkFaces.west, chunkFaces.down, chunkFaces.south, chunkFaces.x, chunkFaces.z, coordsX, coordsY, coordsZ, listFacesX, listFacesY, listFacesZ, listColoursX, listColoursY, listColoursZ, listColoursXA, listColoursYA, listColoursZA)
                End If
            End If
        Next
        allChunkFaces(1, 1) = chunkFaces
    End Sub

    Private Shared Sub TransferFaceData(ByRef x As ChunkFace, ByRef y As ChunkFace, ByRef z As ChunkFace, xPos As Integer, zPos As Integer, ByRef coordsX As List(Of Single), ByRef coordsY As List(Of Single), ByRef coordsZ As List(Of Single), ByRef listFacesX As List(Of Byte), ByRef listFacesY As List(Of Byte), ByRef listFacesZ As List(Of Byte), ByRef listColoursX As List(Of Byte), ByRef listColoursY As List(Of Byte), ByRef listColoursZ As List(Of Byte), ByRef listColoursXA As List(Of Byte), ByRef listColoursYA As List(Of Byte), ByRef listColoursZA As List(Of Byte))
        z.coords = ProcessCoords(xPos * 16, zPos * 16, coordsZ.ToArray())
        x.coords = ProcessCoords(xPos * 16, zPos * 16, coordsX.ToArray())
        y.coords = ProcessCoords(xPos * 16, zPos * 16, coordsY.ToArray())

        z.blockType = listFacesZ.ToArray()
        x.blockType = listFacesX.ToArray()
        y.blockType = listFacesY.ToArray()

        z.lightNatural = listColoursZ.ToArray()
        x.lightNatural = listColoursX.ToArray()
        y.lightNatural = listColoursY.ToArray()
        z.lightArtificial = listColoursZA.ToArray()
        x.lightArtificial = listColoursXA.ToArray()
        y.lightArtificial = listColoursYA.ToArray()
    End Sub

    Public Shared Function ProcessCoords(offsetX As Integer, offsetZ As Integer, coords As Single()) As Single()
        For i = 0 To coords.Length - 1 Step 4
            coords(i) += offsetX
            coords(i + 2) += offsetZ
        Next
        Return coords
    End Function

End Class

Public Class LightingClass
    Public Shared Sub InitialiseBlockLighting(ByRef lighting As Byte())
        Dim lightData(65536) As Byte
        lighting = lightData
    End Sub

    Public Shared Sub MergeLighting(daylight As Single, ByRef chunk As RenderWorld.ChunkFace, lightingMapping As Byte())
        ReDim chunk.lightFinal(chunk.lightNatural.Length - 1)
        Dim artificialLight As Byte
        For i = 0 To chunk.lightNatural.Length - 1
            'chunk.lightFinal(i) = CByte(chunk.lightNatural(i) * chunk.lightNatural(i) * daylight + 20)
            'artificialLight = CByte(chunk.lightArtificial(i) * chunk.lightArtificial(i) + 20)
            'If artificialLight > chunk.lightFinal(i) Then
            '    chunk.lightFinal(i) = artificialLight
            'End If
            chunk.lightFinal(i) = lightingMapping(chunk.lightNatural(i) * 16 + chunk.lightArtificial(i))
        Next
    End Sub

    Public Shared Function NewSpreadStructure() As List(Of ToSpread)()
        Dim toSpread(15) As List(Of ToSpread)
        For i = 0 To 15
            toSpread(i) = New List(Of ToSpread)
        Next
        Return toSpread
    End Function

    Public Shared Sub GenerateArtificialBlockLighting(ByRef data As Byte(), ByRef lighting As Byte(), ByRef lightLocs As List(Of Integer), ByVal surroundIndex As Integer(), ByRef toSpread As List(Of ToSpread)(), ByVal allLightingRequired As Boolean, ByVal surroundIndexFull As Integer(,), ByVal additionalLights As Boolean, ByRef chunkCoord As RenderWorld.ChunkCoord)
        Dim surround(3)() As List(Of ToSpread)
        Dim overflow As Boolean() = {False, False, False, False}
        Dim overflowEnable As Boolean() = {False, False, False, False}
        Dim reverseXCoords As Integer() = {1, 1, 2, 0}
        Dim reverseZCoords As Integer() = {2, 0, 1, 1}
        Dim surroundChunk As RenderWorld.ChunkFaces
        Dim tempToSpread As ToSpread
        Dim tempSurroundChunkCoord As New RenderWorld.ChunkCoord
        Dim i, j, k, l As Integer
        For j = 0 To 3
            InitialiseToSpread(surround(j))
            If surroundIndex(j) > -1 Then
                surround(j) = RenderWorld.LoadedChunks(surroundIndex(j)).toSpreadArtificial
                overflowEnable(j) = True
            End If
        Next
        If allLightingRequired Then ' Complete refresh
            If Not additionalLights Then
                Array.Clear(lighting, 0, lighting.Length) ' Clear all light
                For k = 0 To 15
                    i = 0
                    While i < toSpread(k).Count
                        If toSpread(k)(i).sourceX = 0 And toSpread(k)(i).sourceZ = 0 Then
                            toSpread(k).RemoveAt(i)
                        Else
                            i += 1
                        End If
                    End While
                Next
                For i = 0 To lightLocs.Count - 1
                    toSpread(14).Add(New LightingClass.ToSpread(0, 0, lightLocs(i)))
                Next
            End If
            For i = 0 To 2
                For j = 0 To 2
                    For k = 0 To 15
                        If surroundIndexFull(i, j) > -1 Then
                            l = 0
                            While l < RenderWorld.LoadedChunks(surroundIndexFull(i, j)).overflowArtificialSaved(k).Count
                                tempToSpread = RenderWorld.LoadedChunks(surroundIndexFull(i, j)).overflowArtificialSaved(k)(l)
                                l += 1
                            End While
                        End If
                    Next
                Next
            Next
        End If
        GenerateBlockLighting(data, lighting, toSpread, surround, overflow, chunkCoord)
        For i = 0 To 3
            If overflow(i) And overflowEnable(i) Then
                surroundChunk = RenderWorld.LoadedChunks(surroundIndex(i))
                RenderWorld.LoadedChunks(surroundIndex(i)).overflowArtificial = True
                tempSurroundChunkCoord = New RenderWorld.ChunkCoord(surroundChunk.x, surroundChunk.z)
                If Not RenderWorld.DuplicateCoordsToGenerate(tempSurroundChunkCoord, RenderWorld.GenerateFaceList) Then
                    RenderWorld.GenerateFaceList.Add(tempSurroundChunkCoord)
                End If
                If Not RenderWorld.DuplicateCoordsToGenerate(tempSurroundChunkCoord, RenderWorld.GenerateLightingArtificialList) Then
                    RenderWorld.GenerateLightingArtificialList.Add(tempSurroundChunkCoord)
                End If
            End If
        Next
    End Sub

    Public Shared Sub GenerateNaturalBlockLighting(ByRef data As Byte(), ByRef lighting As Byte(), ByVal surroundIndex As Integer(), ByRef toSpread As List(Of ToSpread)(), ByVal allLightingRequired As Boolean, ByVal surroundIndexFull As Integer(,), ByRef chunkCoord As RenderWorld.ChunkCoord)
        Dim surround(3)() As List(Of ToSpread)
        Dim overflow As Boolean() = {False, False, False, False}
        Dim overflowEnable As Boolean() = {False, False, False, False}
        Dim oecopy As Boolean() = {False, False, False, False}
        Dim reverseXCoords As Integer() = {1, 1, 2, 0}
        Dim reverseZCoords As Integer() = {2, 0, 1, 1}
        Dim surroundChunk As RenderWorld.ChunkFaces
        Dim tempToSpread As ToSpread
        Dim i, j, k, l As Integer
        For j = 0 To 3
            InitialiseToSpread(surround(j))
            If surroundIndex(j) > -1 Then
                surround(j) = RenderWorld.LoadedChunks(surroundIndex(j)).toSpreadNatural
                overflowEnable(j) = True
                oecopy(j) = True
            End If
        Next
        If allLightingRequired Then ' Complete refresh
            Array.Clear(lighting, 0, lighting.Length) ' Clear all light
            For k = 0 To 15
                i = 0
                While i < toSpread(k).Count
                    If toSpread(k)(i).sourceX = 0 And toSpread(k)(i).sourceZ = 0 Then
                        toSpread(k).RemoveAt(i)
                    Else
                        i += 1
                    End If
                End While
            Next
            For i = 0 To 15
                For j = 0 To 15
                    toSpread(15).Add(New LightingClass.ToSpread(0, 0, i + 255 * 16 + j * 4096))
                Next
            Next
            For i = 0 To 2
                For j = 0 To 2
                    For k = 0 To 15
                        If surroundIndexFull(i, j) > -1 Then
                            l = 0
                            While l < RenderWorld.LoadedChunks(surroundIndexFull(i, j)).overflowNaturalSaved(k).Count
                                tempToSpread = RenderWorld.LoadedChunks(surroundIndexFull(i, j)).overflowNaturalSaved(k)(l)
                                l += 1
                            End While
                        End If
                    Next
                Next
            Next
        End If
        GenerateBlockLighting(data, lighting, toSpread, surround, overflow, chunkCoord)
        For i = 0 To 3
            If overflow(i) And overflowEnable(i) Then
                surroundChunk = RenderWorld.LoadedChunks(surroundIndex(i))
                RenderWorld.LoadedChunks(surroundIndex(i)).overflowNatural = True
                If Not RenderWorld.DuplicateCoordsToGenerate(New RenderWorld.ChunkCoord(surroundChunk.x, surroundChunk.z), RenderWorld.GenerateFaceList) Then
                    If Math.Abs(surroundChunk.x - Player.chunkX) < RENDERDISTANCE And Math.Abs(surroundChunk.z - Player.chunkZ) < RENDERDISTANCE Then
                        RenderWorld.GenerateFaceList.Add(New RenderWorld.ChunkCoord(surroundChunk.x, surroundChunk.z))
                    End If
                End If
            End If
        Next
    End Sub

    Public Shared Sub InitialiseToSpread(ByRef toSpread As List(Of ToSpread)())
        ReDim toSpread(15)
        For i = 0 To 15
            toSpread(i) = New List(Of ToSpread)
        Next
    End Sub

    Structure ToSpread
        Public sourceX As Integer
        Public sourceZ As Integer
        Public data As Integer
        Sub New(inX As Integer, inZ As Integer, inData As Integer)
            sourceX = inX
            sourceZ = inZ
            data = inData
        End Sub
    End Structure

    Private Shared Sub GenerateBlockLighting(ByRef data As Byte(), ByRef lightData As Byte(), ByRef toSpread As List(Of ToSpread)(), ByRef overflow As List(Of ToSpread)()(), ByRef overflowUsed As Boolean(), ByVal chunkCoord As RenderWorld.ChunkCoord)
        Dim offsetSpread As Integer() = {-4096, 4096, -1, 1, 16, -16}
        Dim spreadOffsetSourceX As Integer() = {0, 0, 1, -1}
        Dim spreadOffsetSourceZ As Integer() = {1, -1, 0, 0}
        Dim overflowCheck As Integer() = {&HF000, &HF000, &HF, &HF, 65536, 65536}
        Dim overflowNewCoords As Integer() = {65536 - 4096, 4096 - 65536, 15, -15}
        Dim toSpreadArr(65535) As Integer
        Dim newSpread As New List(Of Integer)
        Dim blockCheck, newBlockCheck As Integer
        Dim currentSpreadData As New ToSpread
        Dim c As Integer = 0
        Dim count As Integer = 0
        If toSpread(14).Count > 0 Then
            toSpread(14) = toSpread(14)
        End If
        While toSpread(15).Count > 0
            If toSpread(14).Count > 1000 Then
                count = count
            End If
            currentSpreadData = toSpread(15)(toSpread(15).Count - 1)
            blockCheck = currentSpreadData.data
            toSpread(15).RemoveAt(toSpread(15).Count - 1)
            If blockCheck - 16 > 0 Then
                If data(blockCheck - 16) = Terrain.Blocks.air OrElse (data(blockCheck - 16) = Terrain.Blocks.torch And lightData(blockCheck - 16) < 15) Then
                    toSpread(15).Add(New ToSpread(currentSpreadData.sourceX, currentSpreadData.sourceZ, blockCheck - 16))
                    lightData(blockCheck - 16) = 15
                ElseIf data(blockCheck - 16) = Terrain.Blocks.water Then
                    toSpread(14).Add(New ToSpread(currentSpreadData.sourceX, currentSpreadData.sourceZ, blockCheck - 16))
                    lightData(blockCheck - 16) = 10
                End If
            End If
            For j = 0 To 3
                newBlockCheck = blockCheck + offsetSpread(j)
                If (newBlockCheck Or overflowCheck(j)) <> (blockCheck Or overflowCheck(j)) Then
                    overflow(j)(14).Add(New ToSpread(currentSpreadData.sourceX + spreadOffsetSourceX(j), currentSpreadData.sourceZ + spreadOffsetSourceZ(j), blockCheck + overflowNewCoords(j)))
                    overflowUsed(j) = True
                Else
                    toSpread(14).Add(New ToSpread(currentSpreadData.sourceX, currentSpreadData.sourceZ, newBlockCheck))
                End If
            Next
        End While
        For i = 14 To 1 Step -1
            While toSpread(i).Count > 0
                currentSpreadData = toSpread(i)(toSpread(i).Count - 1)
                blockCheck = currentSpreadData.data
                If data(blockCheck) = Terrain.Blocks.air OrElse data(blockCheck) = Terrain.Blocks.torch OrElse data(blockCheck) = Terrain.Blocks.furnace + 1 OrElse data(blockCheck) = Terrain.Blocks.water Then
                    If data(blockCheck) = Terrain.Blocks.water Then
                        blockCheck = blockCheck
                    End If
                    If lightData(blockCheck) < i Then ' If target block is darker
                        lightData(blockCheck) = CByte(i) ' Then light it up and continue to flood fill
                        For j = 0 To 5
                            newBlockCheck = blockCheck + offsetSpread(j)
                            If j > 3 OrElse ((newBlockCheck Or overflowCheck(j)) = (blockCheck Or overflowCheck(j))) Then
                                If newBlockCheck > -1 And newBlockCheck < 65536 Then
                                    toSpread(i - 1).Add(New ToSpread(currentSpreadData.sourceX, currentSpreadData.sourceZ, newBlockCheck))
                                End If
                            Else
                                overflow(j)(i - 1).Add(New ToSpread(currentSpreadData.sourceX + spreadOffsetSourceX(j), currentSpreadData.sourceZ + spreadOffsetSourceZ(j), blockCheck + overflowNewCoords(j)))
                                overflowUsed(j) = True
                            End If
                        Next
                    End If
                End If
                toSpread(i).RemoveAt(toSpread(i).Count - 1)
            End While
        Next
        overflowUsed(0) = overflowUsed(0) And CanSpread(New RenderWorld.ChunkCoord(chunkCoord.x, chunkCoord.z - 1))
        overflowUsed(1) = overflowUsed(1) And CanSpread(New RenderWorld.ChunkCoord(chunkCoord.x, chunkCoord.z + 1))
        overflowUsed(2) = overflowUsed(2) And CanSpread(New RenderWorld.ChunkCoord(chunkCoord.x - 1, chunkCoord.z))
        overflowUsed(3) = overflowUsed(3) And CanSpread(New RenderWorld.ChunkCoord(chunkCoord.x + 1, chunkCoord.z))
    End Sub

    Private Shared Function CanSpread(target As RenderWorld.ChunkCoord) As Boolean
        For i = 0 To RenderWorld.GenerateSourceList.Count - 1
            If Math.Abs(RenderWorld.GenerateSourceList(i).x - target.x) < 2 Then
                If Math.Abs(RenderWorld.GenerateSourceList(i).z - target.z) < 2 Then Return True
            End If
        Next
        Return False
    End Function
End Class

Public Class Player
    Const WALKSPEED As Single = 4.3F
    Const SPRINTSPEED As Single = 5.6F
    Const CROUCHSPEED As Single = 1.3F

    Public Shared targetBlock As RayTracing.Intersection

    Public Shared x As Single
    Public Shared y As Single
    Public Shared z As Single
    Public Shared health As Integer = 20
    Public Shared yStart As Single

    Public Shared velY As Single

    Public Shared chunkX As Integer
    Public Shared chunkZ As Integer

    Public Shared bounce As Single

    Public Shared playerAngle As Single
    Public Shared playerElevation As Single

    Public Shared MiningProgress As Single
    Private Shared oldProgress As Single
    Public Shared Sprinting As Boolean
    Public Shared sprintEnable As Boolean
    Public Shared lastSprintPress As Single

    Public Shared Sub Initialise()
        x = 0.5
        z = 0.5
        y = 250
        chunkX = 0
        chunkZ = 0
        While RenderWorld.GetBlock(CInt(x), CInt(y), CInt(z)) = Terrain.Blocks.air
            y -= 1
        End While
        Sprinting = False
        sprintEnable = False
        lastSprintPress = 0
    End Sub

    Public Shared Sub Move(deltaTime As Single, ByRef blockData As ImportedData.BlockData(), ByRef soundData As Sound.SoundData)
        Dim key As New KeyboardInput.Keys
        Dim oldX, oldY, oldZ As Single
        Dim oldXChunk, oldZChunk As Single
        Dim collisionRange As Single = 0.2
        Dim collision As Boolean = False
        Dim speed As Single = WALKSPEED
        Dim oldFalling As Boolean = CanFallCrouch(x, y, z)
        Dim baseBlock As Byte
        oldX = x
        oldZ = z
        oldY = y
        oldXChunk = chunkX
        oldZChunk = chunkZ
        key = KeyboardInput.GetKeys()
        If key.w Then
            If sprintEnable AndAlso Timer - lastSprintPress < 0.2F Then
                Sprinting = True
            End If
            If sprintEnable Then
                lastSprintPress = CSng(Timer)
            End If
            sprintEnable = False
            If Sprinting Then
                speed = SPRINTSPEED
            End If
        Else
            sprintEnable = True
            Sprinting = False
        End If
        If key.shift Then
            speed = CROUCHSPEED
        End If
        If key.w Then
            z += CSng(deltaTime * speed * Math.Cos(playerAngle))
            x += CSng(deltaTime * speed * Math.Sin(playerAngle))
        End If
        If key.a Then
            z += CSng(deltaTime * speed * Math.Sin(playerAngle))
            x -= CSng(deltaTime * speed * Math.Cos(playerAngle))
        End If
        If key.s Then
            z -= CSng(deltaTime * speed * Math.Cos(playerAngle))
            x -= CSng(deltaTime * speed * Math.Sin(playerAngle))
        End If
        If key.d Then
            z -= CSng(deltaTime * speed * Math.Sin(playerAngle))
            x += CSng(deltaTime * speed * Math.Cos(playerAngle))
        End If
        'If key.w Or key.a Or key.s Or key.d Then
        '    bounce += deltaTime
        'Else
        '    bounce = 0
        'End If
        If x >= 16 Then
            chunkX += CInt(x) \ 16
            x = x Mod 16
        End If
        While x < 0
            x += 16
            chunkX -= 1
        End While
        If z >= 16 Then
            chunkZ += CInt(z) \ 16
            z = z Mod 16
        End If
        While z < 0
            z += 16
            chunkZ -= 1
        End While
        For i = -1 To 1
            For j = -1 To 1
                If Not CanMoveThroughBlock(RenderWorld.GetBlock(CInt(Math.Floor(x + i * collisionRange + chunkX * 16)), CInt(Math.Floor(y)), CInt(Math.Floor(z + j * collisionRange + chunkZ * 16)))) Or Not CanMoveThroughBlock(RenderWorld.GetBlock(CInt(Math.Floor(x + i * collisionRange + chunkX * 16)), CInt(Math.Floor(y + 1)), CInt(Math.Floor(z + j * collisionRange + chunkZ * 16)))) Or Not CanMoveThroughBlock(RenderWorld.GetBlock(CInt(Math.Floor(x + i * collisionRange + chunkX * 16)), CInt(Math.Floor(y + 1.8F)), CInt(Math.Floor(z + j * collisionRange + chunkZ * 16)))) Then
                    collision = True
                    If i = 0 Then
                        z = oldZ
                        chunkZ = CInt(oldZChunk)
                    Else
                        x = oldX
                        chunkX = CInt(oldXChunk)
                    End If
                End If
            Next
        Next
        If oldX <> x Or oldZ <> z Then
            bounce += deltaTime
        Else
            bounce = 0
        End If
        If Not CanMoveThroughBlock(GetBlock(x, y, z)) Then
            y += CSng(0.01)
        End If
        If collision Then
            If x - Math.Floor(x) < 0.2 Then
                x = CSng(Math.Floor(x) + 0.2)
            End If
            If x - Math.Floor(x) > 0.8 Then
                x = CSng(Math.Floor(x) + 0.8)
            End If
            If z - Math.Floor(z) < 0.2 Then
                z = CSng(Math.Floor(z) + 0.2)
            End If
            If z - Math.Floor(z) > 0.8 Then
                z = CSng(Math.Floor(z) + 0.8)
            End If
        End If

        If key.shift Then
            If CanFallCrouch(x, y, z) And Not oldFalling Then
                x = oldX
                y = oldY
                z = oldZ
            End If
        End If

        If Math.Floor(x) <> Math.Floor(oldX) OrElse Math.Floor(z) <> Math.Floor(oldZ) Then
            baseBlock = RenderWorld.GetBlock(CInt(Math.Floor(x + chunkX * 16)), CInt(Math.Floor(y - 0.1F)), CInt(Math.Floor(z + chunkZ * 16)))
            If Not baseBlock = Terrain.Blocks.water Or Rnd() > 0.9F Then
                Sound.PlayWalkSound(blockData(baseBlock).Sound, soundData)
            End If
        End If
    End Sub

    Private Shared Function CanFallCrouch(checkX As Single, checkY As Single, checkZ As Single) As Boolean
        Dim fall As Boolean = True
        For i = -1 To 1
            For j = -1 To 1
                fall = fall And CanFall(checkX + 0.5F * i, checkY, checkZ + 0.5F * j)
            Next
        Next
        Return fall
    End Function

    Private Shared Function CanFall(checkX As Single, checkY As Single, checkZ As Single) As Boolean
        Dim baseBlock As Integer
        baseBlock = RenderWorld.GetBlock(CInt(Math.Floor(checkX + chunkX * 16)), CInt(Math.Floor(checkY - 0.1F)), CInt(Math.Floor(checkZ + chunkZ * 16)))
        Return baseBlock = Terrain.Blocks.air Or baseBlock = Terrain.Blocks.torch
    End Function

    Public Shared Function CanMoveThroughBlock(collision As Integer) As Boolean
        If collision = Terrain.Blocks.air Then Return True
        If collision = Terrain.Blocks.torch Then Return True
        If collision = Terrain.Blocks.water Then Return True
        Return False
    End Function

    Public Shared Sub Jump()
        Dim key As KeyboardInput.Keys = KeyboardInput.GetKeys
        If key.space Then
            If ((Not CanFallCrouch(x, y - 0.1F, z) And key.shift) Or Not CanFall(x, y - 0.1F, z)) And velY < 0.1 Then
                If GetBlock(x, y - 0.1F, z) = Terrain.Blocks.water Then
                    If GetBlock(x, y + 0.5F, z) = Terrain.Blocks.water Then
                        velY = 3.5
                    End If
                Else
                    velY = 5.5
                End If
            End If
        End If
    End Sub

    Public Shared Sub ApplyGravity(deltaTime As Single)
        Dim deltaY, deltaYCheck As Single
        Dim baseBlock As Integer
        Dim crouch As Boolean = KeyboardInput.GetKeys().shift
        If Not CanMoveThroughBlock(GetBlock(x, y + 1.9F, z)) Then
            velY *= -0.5F
        End If
        baseBlock = GetBlock(x, y - 0.1F, z)
        If CanFall(x, y, z) And Not crouch Or CanFallCrouch(x, y, z) Then
            velY -= deltaTime * 13
            If velY < -10 Then velY = -10
        ElseIf baseBlock = Terrain.Blocks.water Then
            velY -= deltaTime * 5
            If velY < -5 Then velY = -5
        Else
            If velY < 0.1 Then
                y = CSng(Math.Floor(y) + 0.05)
                velY = 0
            End If
        End If
        deltaY = deltaTime * velY
        If deltaY < 0 Then
            deltaYCheck = 0
            While deltaYCheck > deltaY
                deltaYCheck -= 1
                If deltaYCheck < deltaY Then deltaYCheck = deltaY
                baseBlock = GetBlock(x, y - 0.1F + deltaYCheck, z)
                If Not CanMoveThroughBlock(baseBlock) Then
                    deltaY = deltaYCheck
                End If
            End While
        End If
        y += deltaY

        If y > yStart Then
            yStart = y
        End If

        baseBlock = GetBlock(x, y, z)
        If Not CanMoveThroughBlock(baseBlock) Then
            y = CSng(Math.Ceiling(y) + 0.05)
            If y < yStart - 3 And velY < 0 Then
                health -= CInt(yStart - y - 3)
            End If
            yStart = y
        End If
        If baseBlock = Terrain.Blocks.water Then
            If y < yStart Then
                yStart = y
            End If
        End If
    End Sub

    Public Shared Function GetBlock(xBlock As Single, yBlock As Single, zBlock As Single) As Byte
        Try
            Return RenderWorld.LoadedChunks(RenderWorld.GetIndexOfChunkData(chunkX, chunkZ)).data(CInt(Math.Floor(zBlock) * 4096 + Math.Floor(yBlock) * 16 + Math.Floor(xBlock)))
        Catch
            Return 0
        End Try
    End Function

    Public Shared Sub Rotate()
        Dim mouse As New MouseInput.MouseInput
        mouse = MouseInput.GetInput()
        playerAngle += CSng((mouse.x - 20) / 200)
        playerElevation += CSng((20 - mouse.y) / 200)
        If playerElevation > Math.PI / 2 Then playerElevation = Math.PI / 2
        If playerElevation < -Math.PI / 2 Then playerElevation = -Math.PI / 2
        playerAngle += CSng(Math.PI * 2)
        playerAngle = CSng(playerAngle Mod (Math.PI * 2))
        MouseInput.ForceMouseMove(20, 20)
    End Sub

    Private Shared Function NearestZombie(ByRef zombieData As Zombie(), ByVal referenceBlock As RayTracing.Intersection) As Integer
        Dim minDistance As Single = DistanceSquared(Player.x + Player.chunkX * 16, Player.y, Player.z + Player.chunkZ * 16, referenceBlock.xSng, referenceBlock.ySng, referenceBlock.zSng)
        Dim newDistance As Single
        Dim index As Integer = -1
        For i = 0 To zombieData.Length - 1
            If zombieData(i).inUse Then
                If RayTracing.PointingAtCuboid(Player.playerAngle, Player.playerElevation, Player.x + Player.chunkX * 16, Player.y + 1.8F, Player.z + Player.chunkZ * 16, zombieData(i).baseXCentre, zombieData(i).baseYCentre, zombieData(i).baseZCentre, zombieData(i).orientation, 1, 2, 0.25F) Then
                    newDistance = DistanceSquared(Player.x + Player.chunkX * 16, Player.y, Player.z + Player.chunkZ * 16, zombieData(i).baseXCentre, zombieData(i).baseYCentre, zombieData(i).baseZCentre)
                    If newDistance < minDistance Then
                        minDistance = newDistance
                        index = i
                    End If
                End If
            End If
        Next
        Return index
    End Function

    Private Shared Function DistanceSquared(x As Single, y As Single, z As Single, x2 As Single, y2 As Single, z2 As Single) As Single
        Return (x - x2) * (x - x2) + (y - y2) * (y - y2) + (z - z2) * (z - z2)
    End Function

    Public Shared Sub MineAndPlace(deltaTime As Single, ByRef oldMouseLeftToggle As Boolean, ByRef oldMouseRightToggle As Boolean, ByRef currentInterface As Interfaces, blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), ByRef allChunkChanges As FEN.ChunkChanges(), ByRef furnaceData As Furnace(), ByRef FurnaceIndex As Integer, ByRef zombies As Zombie(), ByRef soundData As Sound.SoundData)
        Dim mouse As New MouseInput.MouseInput
        Dim oldBlock As New RayTracing.Intersection(targetBlock)
        Dim validTrace As Boolean = True
        Dim droppedItem As Integer
        Dim hardnessModified As Single
        Dim blockTargetID As Integer
        Dim targetBlockID As Integer
        Dim targetZombie As Integer
        mouse = MouseInput.GetInput
        targetBlock = RayTracing.Trace(playerAngle, playerElevation, x + chunkX * 16, y + 1.8F, z + chunkZ * 16, 50)
        targetZombie = NearestZombie(zombies, targetBlock)

        If targetZombie > -1 Then targetBlock.failed = True
        If targetBlock.failed Then validTrace = False
        If targetBlock.x <> oldBlock.x Or targetBlock.y <> oldBlock.y Or targetBlock.z <> oldBlock.z Or Not validTrace Then
            MiningProgress = 0
        End If

        If validTrace Then
            If mouse.left Then
                blockTargetID = RenderWorld.GetBlock(targetBlock.x, targetBlock.y, targetBlock.z)
                hardnessModified = blockData(blockTargetID).Hardness
                If blockData(blockTargetID).Pickaxe Then hardnessModified /= (1 + GetToolPower(Inventory.selected, itemData, ToolStatData.Picaxe))
                If blockData(blockTargetID).Axe Then hardnessModified /= (1 + GetToolPower(Inventory.selected, itemData, ToolStatData.Axe))
                If blockData(blockTargetID).Shovel Then hardnessModified /= (1 + GetToolPower(Inventory.selected, itemData, ToolStatData.Shovel))
                If blockData(blockTargetID).MinToolLevel <= GetToolPower(Inventory.selected, itemData, ToolStatData.Picaxe) Then
                    hardnessModified *= 1.5F
                Else
                    hardnessModified *= 5
                End If
                MiningProgress += 1 / hardnessModified * deltaTime
                If (MiningProgress - oldProgress) * hardnessModified > 0.3F Then
                    oldProgress = MiningProgress
                    Sound.PlayWalkSound(blockData(blockTargetID).Sound, soundData)
                End If
                If blockTargetID = Terrain.Blocks.water Then MiningProgress = 0
                If MiningProgress > 1 Then
                    Inventory.LoseDurability(Inventory.hotbar(Inventory.selected), itemData, soundData)
                    droppedItem = RenderWorld.SetBlock(targetBlock.x, targetBlock.y, targetBlock.z, CByte(Terrain.Blocks.air), allChunkChanges, furnaceData)
                    If blockData(droppedItem).MinToolLevel <= GetToolPower(Inventory.selected, itemData, ToolStatData.Picaxe) Then
                        Inventory.PickupItem(blockData(droppedItem).DropID, soundData, itemData)
                    End If
                    MiningProgress = 0
                End If
            Else
                MiningProgress = 0
            End If

            If MiningProgress = 0 Then
                oldProgress = 0
            End If

            targetBlockID = RenderWorld.GetBlock(targetBlock.x, targetBlock.y, targetBlock.z)
            If mouse.rightToggle <> oldMouseRightToggle Then
                Select Case targetBlockID
                    Case Terrain.Blocks.craftingTable
                        currentInterface = Interfaces.Crafting
                    Case Terrain.Blocks.furnace
                        FurnaceIndex = Furnace.GetIndexOfFurnaceMatching(furnaceData, New RenderWorld.ChunkCoord(RenderWorld.CoordsToChunk(targetBlock.x), RenderWorld.CoordsToChunk(targetBlock.z)), RenderWorld.GetRelPositionOfBlock(targetBlock.x, targetBlock.y, targetBlock.z))
                        currentInterface = Interfaces.Furnace
                    Case Terrain.Blocks.furnace + 1
                        FurnaceIndex = Furnace.GetIndexOfFurnaceMatching(furnaceData, New RenderWorld.ChunkCoord(RenderWorld.CoordsToChunk(targetBlock.x), RenderWorld.CoordsToChunk(targetBlock.z)), RenderWorld.GetRelPositionOfBlock(targetBlock.x, targetBlock.y, targetBlock.z))
                        currentInterface = Interfaces.Furnace
                    Case Else
                        If Inventory.hotbar(Inventory.selected).itemID < 100 Then
                            PlaceBlock(allChunkChanges, furnaceData)
                        End If
                End Select
            End If
        Else
            MiningProgress = 0
        End If

        If targetZombie > -1 Then ' HERE
            If mouse.leftToggle <> oldMouseLeftToggle Then
                If DistanceSquared(x + chunkX * 16, y, chunkZ * 16 + z, zombies(targetZombie).baseXCentre, zombies(targetZombie).baseYCentre, zombies(targetZombie).baseZCentre) < 36 Then
                    Inventory.LoseDurability(Inventory.hotbar(Inventory.selected), itemData, soundData)
                    zombies(targetZombie).GetAttacked(GetToolPower(Inventory.selected, itemData, ToolStatData.Sword))
                End If
            End If
        End If

        oldMouseLeftToggle = mouse.leftToggle
        oldMouseRightToggle = mouse.rightToggle

    End Sub

    Private Shared Function GetToolPower(index As Integer, itemData As ImportedData.ItemData(), toolType As ToolStatData) As Integer
        Dim ID As Integer = Inventory.hotbar(index).itemID
        If ID < 100 Then Return 0
        Select Case toolType
            Case ToolStatData.Picaxe
                Return itemData(ID - 100).Picaxe
            Case ToolStatData.Axe
                Return itemData(ID - 100).Axe
            Case ToolStatData.Shovel
                Return itemData(ID - 100).Shovel
            Case ToolStatData.Sword
                Return itemData(ID - 100).Sword
        End Select
        Return 0
    End Function

    Public Enum ToolStatData
        Picaxe = 0
        Axe = 1
        Shovel = 2
        Sword = 3
    End Enum

    Public Shared PlacedAtTime As Double

    Private Shared Sub PlaceBlock(ByRef allChunkChanges As FEN.ChunkChanges(), ByRef furnaceData As Furnace())
        Dim blockTarget As CustomBlocks.TorchBlockAttached
        Dim blockTargetID As Integer
        Dim blockToPlace As Integer
        blockToPlace = Inventory.hotbar(Inventory.selected).itemID
        blockTargetID = RenderWorld.GetBlock(targetBlock.x, targetBlock.y, targetBlock.z)
        PlacedAtTime = Timer
        If Not (blockTargetID = Terrain.Blocks.torch And blockToPlace = Terrain.Blocks.torch) Then
            Select Case targetBlock.plane
                Case RayTracing.Faces.up
                    targetBlock.y += 1
                    blockTarget = CustomBlocks.TorchBlockAttached.down
                Case RayTracing.Faces.down
                    targetBlock.y -= 1
                Case RayTracing.Faces.backwards
                    targetBlock.z += 1
                    blockTarget = CustomBlocks.TorchBlockAttached.backward
                Case RayTracing.Faces.forward
                    targetBlock.z -= 1
                    blockTarget = CustomBlocks.TorchBlockAttached.forward
                Case RayTracing.Faces.left
                    targetBlock.x += 1
                    blockTarget = CustomBlocks.TorchBlockAttached.left
                Case RayTracing.Faces.right
                    targetBlock.x -= 1
                    blockTarget = CustomBlocks.TorchBlockAttached.right
            End Select

            blockTargetID = RenderWorld.GetBlock(targetBlock.x, targetBlock.y, targetBlock.z)
            If blockTargetID = Terrain.Blocks.air Or blockTargetID = Terrain.Blocks.water Then
                If Inventory.hotbar(Inventory.selected).numberOfItems > 0 And blockToPlace <> Terrain.Blocks.air Then
                    RenderWorld.SetBlock(targetBlock.x, targetBlock.y, targetBlock.z, CByte(blockToPlace), allChunkChanges, furnaceData, blockTarget)
                    Inventory.hotbar(Inventory.selected).numberOfItems -= CByte(1)
                End If
                If Inventory.hotbar(Inventory.selected).numberOfItems = 0 Then
                    Inventory.hotbar(Inventory.selected).itemID = CByte(Terrain.Blocks.air)
                End If
            End If
        End If
    End Sub
End Class

Public Class Furnace

    Public FurnaceBurn As Boolean = False
    Public oldBurn As Boolean = False
    Public fireTime As Integer
    Public fireTimeStart As Integer
    Public smeltTime As Integer
    Public smeltTimeStart As Integer
    Public output As Inventory.InventoryItem
    Public fuel As Inventory.InventoryItem
    Public smelt As Inventory.InventoryItem
    Public myCoord As Integer
    Public chunkX As Integer
    Public chunkZ As Integer
    Public inUse As Boolean

    Public Function OutputData() As String
        Dim data As New Text.StringBuilder
        data.Append(If(FurnaceBurn, "1", "0"))
        data.Append(If(oldBurn, "1", "0"))
        data.Append(If(inUse, "1", "0"))
        data.Append(",")
        data.Append(fireTime & ",")
        data.Append(fireTimeStart & ",")
        data.Append(smeltTime & ",")
        data.Append(smeltTimeStart & ",")
        data.Append(myCoord & ",")
        data.Append(chunkX & ",")
        data.Append(chunkZ & ",")
        data.Append(OutputInventoryItem(output) & ",")
        data.Append(OutputInventoryItem(fuel) & ",")
        data.Append(OutputInventoryItem(smelt) & ",")
        Return data.ToString
    End Function

    Public Function OutputInventoryItem(item As Inventory.InventoryItem) As String
        Dim data As New Text.StringBuilder
        data.Append(item.itemID)
        data.Append(".")
        data.Append(item.numberOfItems)
        data.Append(".")
        data.Append(item.durability)
        Return data.ToString
    End Function

    Public Sub InputInventoryItem(ByRef item As Inventory.InventoryItem, data As String)
        Dim splitData As String() = data.Split("."c)
        item.itemID = CByte(splitData(0))
        item.numberOfItems = CByte(splitData(1))
        item.durability = CInt(splitData(2))
    End Sub

    Public Sub InputData(data As String)
        Dim splitData As String() = data.Split(","c)
        FurnaceBurn = splitData(0)(0) = "1"c
        oldBurn = splitData(0)(1) = "1"c
        inUse = splitData(0)(2) = "1"c
        fireTime = CInt(splitData(1))
        fireTimeStart = CInt(splitData(2))
        smeltTime = CInt(splitData(3))
        smeltTimeStart = CInt(splitData(4))
        myCoord = CInt(splitData(5))
        chunkX = CInt(splitData(6))
        chunkZ = CInt(splitData(7))
        InputInventoryItem(output, splitData(8))
        InputInventoryItem(fuel, splitData(9))
        InputInventoryItem(smelt, splitData(10))
    End Sub


    Public Shared Function GetIndexOfFurnaceMatching(ByRef furnaceData As Furnace(), chunkCoords As RenderWorld.ChunkCoord, relPosition As Integer) As Integer
        For i = 0 To furnaceData.Length - 1
            If furnaceData(i).chunkX = chunkCoords.x And furnaceData(i).chunkZ = chunkCoords.z And furnaceData(i).inUse And furnaceData(i).myCoord = relPosition Then
                Return i
            End If
        Next
        Return 0
    End Function

    Public Shared Function InRange(furnaceData As Furnace) As Boolean
        If Not IsNothing(furnaceData) Then
            If Math.Abs(furnaceData.chunkX - Player.chunkX) < RENDERDISTANCE And Math.Abs(furnaceData.chunkZ - Player.chunkZ) < RENDERDISTANCE And furnaceData.inUse Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Function GetFireTime(ByRef fuelData As ImportedData.FuelData(), itemID As Byte) As Integer
        For i = 0 To fuelData.Length - 1
            If fuelData(i).ID = itemID Then Return fuelData(i).burnTime
        Next
        Return 0
    End Function

    Public Function GetSmeltTime(ByRef smeltData As ImportedData.SmeltData(), itemID As Byte) As Integer
        For i = 0 To smeltData.Length - 1
            If smeltData(i).ID = itemID Then Return smeltData(i).smeltTime
        Next
        Return 0
    End Function

    Public Function GetSmeltProduct(ByRef smeltData As ImportedData.SmeltData(), itemID As Byte) As Byte
        For i = 0 To smeltData.Length - 1
            If smeltData(i).ID = itemID Then Return smeltData(i).outputID
        Next
        Return 0
    End Function

    Public Sub Tick(ByRef fuelData As ImportedData.FuelData(), ByRef smeltData As ImportedData.SmeltData(), ByRef allChunkData As FEN.ChunkChanges())
        smeltTime -= 1
        fireTime -= 1
        If smeltTime <> smeltTimeStart And Not FurnaceBurn Then
            smeltTimeStart = GetSmeltTime(smeltData, smelt.itemID)
            smeltTime = smeltTimeStart
        End If
        If fuel.itemID <> 0 Then
            fuelData = fuelData
        End If
        If output.numberOfItems < 1 Then output.itemID = 0
        If fireTime > 0 Then FurnaceBurn = True
        If fireTime < 0 Then
            If fuel.numberOfItems > 0 And smelt.numberOfItems > 0 Then
                fuel.numberOfItems -= CByte(1)
                fireTimeStart = GetFireTime(fuelData, fuel.itemID)
                fireTime = fireTimeStart
            Else
                FurnaceBurn = False
            End If
        End If
        If fireTime > 0 Then FurnaceBurn = True
        If smeltTime < 0 And fireTime > 0 And smelt.numberOfItems > 0 And smelt.itemID <> Terrain.Blocks.air Then
            output.itemID = GetSmeltProduct(smeltData, smelt.itemID)
            output.numberOfItems += CByte(1)
            If output.itemID = 0 Or output.itemID = GetSmeltProduct(smeltData, smelt.itemID) Then
                smeltTimeStart = GetSmeltTime(smeltData, smelt.itemID)
                smeltTime = smeltTimeStart
                If fireTime > 0 And smelt.numberOfItems > 0 Then
                    smelt.numberOfItems -= CByte(1)
                End If
            End If
        End If
        If smelt.numberOfItems = 0 Then
            smelt.itemID = CByte(Terrain.Blocks.air)
        End If
        If fuel.numberOfItems = 0 Then
            fuel.itemID = CByte(Terrain.Blocks.air)
        End If
        If Not FurnaceBurn Then fireTime = 0
        If oldBurn <> FurnaceBurn Then
            'CHANGE FURNACE BLOCK AND LIGHTING
            If FurnaceBurn Then
                RenderWorld.SetBlock(myCoord Mod 16 + chunkX * 16, (myCoord And &HFF0) \ 16, (myCoord And &HF000) \ 4096 + chunkZ * 16, Terrain.Blocks.furnace + 1, allChunkData, {}, 0, True)
            Else
                RenderWorld.SetBlock(myCoord Mod 16 + chunkX * 16, (myCoord And &HFF0) \ 16, (myCoord And &HF000) \ 4096 + chunkZ * 16, CByte(Terrain.Blocks.furnace), allChunkData, {}, 0, True)
            End If
        End If
        oldBurn = FurnaceBurn
    End Sub

End Class

Public Class Inventory
    Public Shared inventory(26) As InventoryItem
    Public Shared hotbar(8) As InventoryItem
    Public Shared holding As InventoryItem
    Public Shared selected As Integer
    Public Shared crafting(2, 2) As InventoryItem
    Public Shared craftingOutput As InventoryItem

    Const hotbarSize As Single = 0.15
    Const itemInHotbarSize As Single = 0.7
    Const heartOffset As Single = 0.4F
    Const heartSize As Single = 2
    Const numberSize As Single = 0.4
    Const crosshairSize As Single = 0.1

    Public Shared Sub LoseDurability(ByRef item As InventoryItem, ByRef itemData As ImportedData.ItemData(), ByRef soundData As Sound.SoundData)
        Dim id As Integer = item.itemID - 100
        If item.itemID < 100 Then Return
        If itemData(id).Durability = 0 Then Return
        item.durability -= 1
        If item.durability <= 0 Then
            item.itemID = CByte(Terrain.Blocks.air)
            item.numberOfItems = 0
            Sound.PlayAdditionalSound("break", soundData)
        End If
    End Sub

    Public Shared Function MaxStack(itemData As ImportedData.ItemData(), index As Byte) As Integer
        If index < 100 Then Return 64
        If itemData(index - 100).Durability = 0 Then Return 64
        Return 1
    End Function

    Public Shared Sub PickupItem(item As Byte, ByRef soundData As Sound.SoundData, ByRef itemData As ImportedData.ItemData())
        Dim tryPlace As Integer = 0
        Dim placeFound As Boolean = False
        While tryPlace < 9 And Not placeFound
            If hotbar(tryPlace).itemID = 0 Or hotbar(tryPlace).numberOfItems = 0 Or (hotbar(tryPlace).itemID = item And hotbar(tryPlace).numberOfItems < MaxStack(itemData, item)) Then
                placeFound = True
                If hotbar(tryPlace).itemID = 0 Then hotbar(tryPlace).numberOfItems = 0
                hotbar(tryPlace).itemID = item
                hotbar(tryPlace).numberOfItems += CByte(1)
            End If
            tryPlace += 1
        End While
        tryPlace = 0
        While tryPlace < inventory.Length - 1 And Not placeFound
            If inventory(tryPlace).itemID = 0 Or inventory(tryPlace).numberOfItems = 0 Or (inventory(tryPlace).itemID = item And inventory(tryPlace).numberOfItems < MaxStack(itemData, item)) Then
                placeFound = True
                If inventory(tryPlace).itemID = 0 Then inventory(tryPlace).numberOfItems = 0
                inventory(tryPlace).itemID = item
                inventory(tryPlace).numberOfItems += CByte(1)
            End If
            tryPlace += 1
        End While
        Sound.PlayAdditionalSound("pop", soundData)
    End Sub

    Public Shared Sub ChangeSelection()
        Dim key As KeyboardInput.Keys = KeyboardInput.GetKeys()
        For i = 1 To 9
            If key.numbers(i) Then
                If selected <> i - 1 Then Player.MiningProgress = 0
                selected = i - 1
            End If
        Next
    End Sub

    Public Shared Function GetInventorySelected(interfaceMode As Interfaces) As InventorySelected
        Dim pointerLoc As Single()
        Dim mouse As MouseInput.MouseInput = MouseInput.GetInput
        Dim returnValue As New InventorySelected
        returnValue.type = InventoryTypes.NoneSelected
        pointerLoc = MouseInput.GetRelativeLoc(mouse.x, mouse.y)
        pointerLoc(0) *= CSng(Window.GetSize().x / Window.GetSize().y)
        If pointerLoc(0) > hotbarSize * -4.5 And pointerLoc(0) < hotbarSize * 4.5 Then ' x coord in range inventory or hotbar
            If pointerLoc(1) > hotbarSize * -3.5 And pointerLoc(1) < hotbarSize * -2.5 Then
                returnValue.type = InventoryTypes.Hotbar
                returnValue.y = 0
                returnValue.x = CInt(Math.Floor((pointerLoc(0) - hotbarSize * -4.5) / hotbarSize))
            End If
            If pointerLoc(1) > hotbarSize * -2 And pointerLoc(1) < hotbarSize * 1 Then
                returnValue.type = InventoryTypes.Inventory
                returnValue.y = CInt(Math.Floor((hotbarSize - pointerLoc(1)) / hotbarSize))
                returnValue.x = CInt(Math.Floor((pointerLoc(0) - hotbarSize * -4.5) / hotbarSize))
            End If
            If interfaceMode = Interfaces.Crafting Or interfaceMode = Interfaces.Inventory Then
                If pointerLoc(1) > hotbarSize * 0.5 And pointerLoc(1) < hotbarSize * 4.5 And pointerLoc(0) < 0 Then
                    returnValue.type = InventoryTypes.Crafting
                    returnValue.y = CInt(Math.Floor((-pointerLoc(1) + hotbarSize * 4.5) / hotbarSize))
                    returnValue.x = CInt(Math.Floor((pointerLoc(0) + hotbarSize * 4.5) / hotbarSize))
                End If
                If pointerLoc(1) > hotbarSize * 2.5 And pointerLoc(1) < hotbarSize * 3.5 And pointerLoc(0) < hotbarSize * 1.5 And pointerLoc(0) > hotbarSize * 0.5 Then
                    returnValue.type = InventoryTypes.CraftingOutput
                    returnValue.y = 0
                    returnValue.x = 0
                End If
            End If
            If interfaceMode = Interfaces.Furnace Then
                If pointerLoc(1) > hotbarSize * 2.5 And pointerLoc(1) < hotbarSize * 3.5 And pointerLoc(0) < hotbarSize * 1.5 And pointerLoc(0) > hotbarSize * 0.5 Then
                    returnValue.type = InventoryTypes.FurnaceOutput
                    returnValue.y = 0
                    returnValue.x = 0
                End If
                If pointerLoc(0) < hotbarSize * -2.5 And pointerLoc(0) > hotbarSize * -3.5 Then
                    If pointerLoc(1) > hotbarSize * 4.5 And pointerLoc(1) < hotbarSize * 5.5 Then
                        returnValue.type = InventoryTypes.FurnaceSmelt
                    ElseIf pointerLoc(1) > hotbarSize * 2.5 And pointerLoc(1) < hotbarSize * 3.5 Then
                        returnValue.type = InventoryTypes.FurnaceFuel
                    End If
                End If
            End If
        End If
        Return returnValue
    End Function

    Public Shared Sub WriteText(ByRef coords As List(Of Single), ByRef texture As List(Of Byte), words As String, ByRef colours As List(Of Byte), size As Single, baseY As Single)
        Dim xCoord As Single = -1 * size * words.Length / 2
        Dim validLetters As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        words = words.ToUpper
        For i = 0 To words.Length - 1
            If validLetters.Contains(words(i)) Then
                DrawLetter(coords, texture, words(i), xCoord + size * i, baseY, colours, size)
            End If
        Next
    End Sub

    Public Shared Sub DrawLetter(ByRef coords As List(Of Single), ByRef texture As List(Of Byte), letter As Char, baseX As Single, baseY As Single, ByRef colours As List(Of Byte), size As Single)
        Dim baseNumberTexture As Integer = RenderWorld.NumTexturesBlocks + RenderWorld.NumTexturesItems + 47
        Dim coordsOffsetX As Single() = {0, 0, 1, 1}
        Dim coordsOffsetY As Single() = {0, 1, 1, 0}
        For i = 0 To 3
            coords.Add(CSng((coordsOffsetX(i)) * size + baseX))
            coords.Add(coordsOffsetY(i) * size + baseY)
            coords.Add(0)
            coords.Add(1)
        Next
        texture.Add(CByte(baseNumberTexture + AscW(letter) - AscW("A"c)))
        For i = 0 To 11
            colours.Add(255)
        Next
    End Sub

    Private Shared Sub DrawNumber(ByRef coords As List(Of Single), ByRef texture As List(Of Byte), number As Byte, baseX As Single, baseY As Single, ByRef colours As List(Of Byte))
        Dim baseNumberTexture As Integer = RenderWorld.NumTexturesBlocks + RenderWorld.NumTexturesItems + 37
        Dim coordsOffsetX As Single() = {0, 0, 1, 1}
        Dim coordsOffsetY As Single() = {0, 1, 1, 0}
        baseX += (1 - itemInHotbarSize) / 6 * 5 * hotbarSize
        baseY += (1 - itemInHotbarSize) / 2 * hotbarSize
        If number < 2 Then Return
        For j = 0 To 1
            If number > 0 Then
                For i = 0 To 3
                    coords.Add(CSng((coordsOffsetX(i) - j * 0.7 + 1) * itemInHotbarSize * hotbarSize * numberSize + baseX))
                    coords.Add(coordsOffsetY(i) * itemInHotbarSize * hotbarSize * numberSize + baseY)
                    coords.Add(0)
                    coords.Add(1)
                Next
                texture.Add(CByte(baseNumberTexture + number Mod 10))
                For i = 0 To 11
                    colours.Add(255)
                Next
                number \= CByte(10)
            End If
        Next
    End Sub

    Private Shared Sub DrawIcon(ByRef coords As List(Of Single), ByRef texture As List(Of Byte), blockId As Byte, baseX As Single, baseY As Single, blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), inventItem As InventoryItem, ByRef colours As List(Of Byte))
        Dim coordsOffsetX, coordsOffsetY As Single(,)
        baseX += (1 - itemInHotbarSize) / 2 * hotbarSize
        baseY += (1 - itemInHotbarSize) / 2 * hotbarSize
        If blockId < RenderWorld.NumTexturesBlocks And blockId <> Terrain.Blocks.torch Then
            coordsOffsetX = {{0.5, 0.5, 0.933, 0.933}, {0.067, 0.067, 0.5, 0.5}, {0.067, 0.5, 0.933, 0.5}}
            coordsOffsetY = {{0, 0.5, 0.75, 0.25}, {0.25, 0.75, 0.5, 0}, {0.75, 1.0, 0.75, 0.5}}
            For j = 0 To 2
                For i = 0 To 3
                    coords.Add(coordsOffsetX(j, i) * itemInHotbarSize * hotbarSize + baseX)
                    coords.Add(coordsOffsetY(j, i) * itemInHotbarSize * hotbarSize + baseY)
                    coords.Add(0)
                    coords.Add(1)
                Next
            Next
            texture.Add(blockData(blockId).FacesIndex(4))
            texture.Add(blockData(blockId).FacesIndex(2))
            texture.Add(blockData(blockId).FacesIndex(0))
            For i = 0 To 35
                colours.Add(255)
            Next
        ElseIf blockId > 100 Or blockId = Terrain.Blocks.torch Then
            coordsOffsetX = {{0, 0, 1, 1}}
            coordsOffsetY = {{0, 1, 1, 0}}
            For i = 0 To 3
                coords.Add(coordsOffsetX(0, i) * itemInHotbarSize * hotbarSize + baseX)
                coords.Add(coordsOffsetY(0, i) * itemInHotbarSize * hotbarSize + baseY)
                coords.Add(0)
                coords.Add(1)
            Next
            If blockId = Terrain.Blocks.torch Then
                texture.Add(blockData(Terrain.Blocks.torch).FacesIndex(3))
            Else
                texture.Add(CByte(itemData(blockId - 100).TextureIndex))
            End If
            For i = 0 To 11
                colours.Add(255)
            Next
        End If
        DrawDurability(coords, texture, inventItem, baseX, baseY, itemData, colours)
    End Sub

    Private Shared Sub DrawDurability(ByRef coords As List(Of Single), ByRef texture As List(Of Byte), blockData As InventoryItem, baseX As Single, baseY As Single, itemData As ImportedData.ItemData(), ByRef colours As List(Of Byte))
        Dim id As Integer = blockData.itemID - 100
        Dim baseTexture As Byte = CByte(RenderWorld.NumTexturesBlocks + RenderWorld.NumTexturesItems + 75)
        If blockData.itemID < 100 Then Return
        If itemData(id).Durability = 0 Then Return
        If itemData(id).Durability = blockData.durability Then Return
        Dim coordsOffsetX As Single() = {0, 0, 1, 1}
        Dim coordsOffsetY As Single() = {0, 0.1F, 0.1F, 0}
        Dim scale As Single = CSng(Math.Ceiling(blockData.durability / itemData(id).Durability * 16) / 16)
        For i = 0 To 3
            coords.Add(coordsOffsetX(i) * itemInHotbarSize * hotbarSize + baseX)
            coords.Add(coordsOffsetY(i) * itemInHotbarSize * hotbarSize + baseY)
            coords.Add(0)
            coords.Add(1)
        Next
        For i = 0 To 3
            coords.Add(coordsOffsetX(i) * itemInHotbarSize * hotbarSize * scale + baseX)
            coords.Add(coordsOffsetY(i) * itemInHotbarSize * hotbarSize + baseY)
            coords.Add(0)
            coords.Add(1)
        Next
        texture.Add(CByte(RenderWorld.ZombieTextureStart - 1))
        For i = 0 To 11
            colours.Add(0)
        Next
        texture.Add(CByte(RenderWorld.ZombieTextureStart - 1))
        For i = 0 To 3
            colours.Add(CByte(255 * (1 - scale)))
            colours.Add(CByte(scale * 255))
            colours.Add(0)
        Next
    End Sub

    Public Shared Function MoveItem(selected As InventorySelected, oldStates As MouseInput.MouseInput, craftingGridSize As Integer, ByRef furnaceData As Furnace, ByRef itemData As ImportedData.ItemData()) As MouseInput.MouseInput
        Dim mouse As MouseInput.MouseInput = MouseInput.GetInput
        If mouse.left And mouse.leftToggle <> oldStates.leftToggle Then
            Select Case selected.type
                Case InventoryTypes.Inventory
                    SwapItems(inventory(selected.x + selected.y * 9), holding, itemData)
                Case InventoryTypes.Hotbar
                    SwapItems(hotbar(selected.x), holding, itemData)
                Case InventoryTypes.Crafting
                    If craftingGridSize > selected.x And craftingGridSize > selected.y And selected.x >= 0 And selected.y >= 0 Then
                        SwapItems(crafting(selected.x, selected.y), holding, itemData)
                    End If
                Case InventoryTypes.CraftingOutput
                    If holding.numberOfItems + craftingOutput.numberOfItems <= MaxStack(itemData, holding.itemID) Then
                        If (holding.itemID = craftingOutput.itemID Or holding.itemID = Terrain.Blocks.air) And craftingOutput.itemID <> Terrain.Blocks.air And craftingOutput.numberOfItems > 0 Then
                            If holding.itemID = craftingOutput.itemID And holding.numberOfItems > 0 Then
                                holding.numberOfItems += craftingOutput.numberOfItems
                            End If
                            If holding.itemID = 0 Or holding.numberOfItems = 0 Then
                                SwapItems(holding, craftingOutput, itemData)
                            End If
                            For i = 0 To 2
                                For j = 0 To 2
                                    If crafting(i, j).numberOfItems > 0 Then
                                        crafting(i, j).numberOfItems -= CByte(1)
                                    End If
                                    If crafting(i, j).numberOfItems = 0 Then
                                        crafting(i, j).itemID = CByte(Terrain.Blocks.air)
                                    End If
                                Next
                            Next
                        End If
                    End If
                Case InventoryTypes.FurnaceSmelt
                    SwapItems(furnaceData.smelt, holding, itemData)
                Case InventoryTypes.FurnaceFuel
                    SwapItems(furnaceData.fuel, holding, itemData)
                Case InventoryTypes.FurnaceOutput ' ADD CONDITION FOR EMPTY LIKE CRAFTING OUTPUT ' Done
                    If holding.numberOfItems + furnaceData.output.numberOfItems <= MaxStack(itemData, furnaceData.output.itemID) Then
                        If (holding.itemID = furnaceData.output.itemID Or holding.itemID = Terrain.Blocks.air) And furnaceData.output.itemID <> Terrain.Blocks.air And furnaceData.output.numberOfItems > 0 Then
                            If holding.itemID = furnaceData.output.itemID And holding.numberOfItems > 0 Then
                                holding.numberOfItems += furnaceData.output.numberOfItems
                            End If
                            If holding.itemID = 0 Or holding.numberOfItems = 0 Then
                                SwapItems(holding, furnaceData.output, itemData)
                            End If
                        End If
                    End If
            End Select
        End If
        If mouse.right And mouse.rightToggle <> oldStates.rightToggle Then
            Select Case selected.type
                Case InventoryTypes.Inventory
                    SwapSingleItem(holding, inventory(selected.x + selected.y * 9), itemData)
                Case InventoryTypes.Hotbar
                    SwapSingleItem(holding, hotbar(selected.x), itemData)
                Case InventoryTypes.Crafting
                    If craftingGridSize > selected.x And craftingGridSize > selected.y Then
                        SwapSingleItem(holding, crafting(selected.x, selected.y), itemData)
                    End If
                Case InventoryTypes.FurnaceFuel
                    SwapSingleItem(holding, furnaceData.fuel, itemData)
                Case InventoryTypes.FurnaceSmelt
                    SwapSingleItem(holding, furnaceData.smelt, itemData)
            End Select
        End If

        Return mouse
    End Function

    Private Shared Sub SwapSingleItem(ByRef from As InventoryItem, ByRef destination As InventoryItem, ByRef itemData As ImportedData.ItemData())
        If from.numberOfItems > 0 And (destination.itemID = 0 Or destination.itemID = from.itemID And destination.numberOfItems < MaxStack(itemData, destination.itemID)) Then
            from.numberOfItems -= CByte(1)
            If destination.numberOfItems = 0 Then
                destination.itemID = CByte(Terrain.Blocks.air)
            End If
            destination.numberOfItems += CByte(1)
            destination.itemID = from.itemID
            If from.numberOfItems = 0 Then
                from.itemID = CByte(Terrain.Blocks.air)
            End If
        End If
    End Sub

    Private Shared Sub SwapItems(ByRef target1 As InventoryItem, ByRef target2 As InventoryItem, ByRef itemData As ImportedData.ItemData())
        Dim temp As InventoryItem
        If target1.itemID = target2.itemID And target1.numberOfItems + target2.numberOfItems <= MaxStack(itemData, target1.itemID) Then
            target1.itemID = 0
            target2.numberOfItems = target1.numberOfItems + target2.numberOfItems
            target1.numberOfItems = 0
        End If
        temp.itemID = target1.itemID
        temp.numberOfItems = target1.numberOfItems
        temp.durability = target1.durability
        target1.itemID = target2.itemID
        target1.numberOfItems = target2.numberOfItems
        target1.durability = target2.durability
        target2.itemID = temp.itemID
        target2.numberOfItems = temp.numberOfItems
        target2.durability = temp.durability
    End Sub

    Private Shared Function ExtractBlockId(data As InventoryItem(,)) As Integer(,)
        Dim blockId(data.GetLength(0), data.GetLength(1)) As Integer
        For i = 0 To data.GetLength(0) - 1
            For j = 0 To data.GetLength(1) - 1
                blockId(i, j) = data(i, j).itemID
            Next
        Next
        Return blockId
    End Function

    Public Shared Sub DisplayFurnace(selected As InventorySelected, ByRef coords As List(Of Single), ByRef texture As List(Of Byte), blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), furnaceData As Furnace, ByRef fractions As List(Of OpenGL.FractionalIcon), ByRef colours As List(Of Byte))
        Dim itemSlotBaseTexture As Byte = CByte(RenderWorld.NumTexturesBlocks + RenderWorld.NumTexturesItems + 75)
        Dim xChange As Single() = {0, 0, 1, 1}
        Dim zChange As Single() = {0, 1, 1, 0}
        Dim item As New InventoryItem
        Dim fraction As New OpenGL.FractionalIcon

        craftingOutput = CraftingClass.Craft(ExtractBlockId(crafting), itemData)

        For i = 0 To 1
            For j = 0 To 3
                coords.Add(hotbarSize * (xChange(j) - 3.5F))
                coords.Add(hotbarSize * (zChange(j) + i * 2 + 2.5F))
                coords.Add(0)
                coords.Add(1)
            Next
            If selected.type = InventoryTypes.FurnaceFuel And i = 0 Or selected.type = InventoryTypes.FurnaceSmelt And i = 1 Then
                texture.Add(CByte(itemSlotBaseTexture - 1))
            Else
                texture.Add(itemSlotBaseTexture)
            End If
            If i = 0 Then
                item = furnaceData.fuel
            Else
                item = furnaceData.smelt
            End If
            If item.itemID > 0 Then
                item = item
            End If
            For j = 0 To 11
                colours.Add(255)
            Next
            DrawIcon(coords, texture, item.itemID, hotbarSize * (-3.5F), hotbarSize * (i * 2 + 2.5F), blockData, itemData, item, colours)
            DrawNumber(coords, texture, item.numberOfItems, hotbarSize * (-3.5F), hotbarSize * (i * 2 + 2.5F), colours)
        Next
        DrawBox(coords, 0.5F, 2.5F)
        If selected.type = InventoryTypes.FurnaceOutput Then
            texture.Add(CByte(itemSlotBaseTexture - 1))
        Else
            texture.Add(itemSlotBaseTexture)
        End If
        For i = 0 To 11
            colours.Add(255)
        Next

        DrawIcon(coords, texture, furnaceData.output.itemID, hotbarSize * 0.5, hotbarSize * 2.5, blockData, itemData, furnaceData.output, colours)
        DrawNumber(coords, texture, furnaceData.output.numberOfItems, hotbarSize * 0.5, hotbarSize * 2.5, colours)

        DrawBox(coords, -3.5F, 3.5F)
        texture.Add(CByte(itemSlotBaseTexture + 3))
        For i = 0 To 11
            colours.Add(255)
        Next

        fraction.index = texture.Count
        fraction.x = 1
        fraction.z = CSng(furnaceData.fireTime / furnaceData.fireTimeStart)
        If furnaceData.fireTimeStart = 0 Then fraction.z = 0
        fraction.z = CSng(fraction.z * 13 / 16 + 2 / 16)
        fraction.z = CSng(Math.Floor(fraction.z * 16) / 16)
        fractions.Add(fraction)
        DrawBox(coords, -3.5F, 3.5F, 1, fraction.z)
        texture.Add(CByte(itemSlotBaseTexture + 4))
        For i = 0 To 11
            colours.Add(255)
        Next

        fraction.index = texture.Count
        fraction.x = CSng(1 - (furnaceData.smeltTime / furnaceData.smeltTimeStart))
        If furnaceData.smeltTimeStart = 0 Then fraction.x = 0
        fraction.x = CSng(fraction.x * 22 / 16 + 1 / 16)
        fraction.x = CSng(Math.Floor(fraction.x * 16) / 16)
        If fraction.x > 1 Then fraction.x = 1
        fraction.z = 1
        fractions.Add(fraction)
        DrawBox(coords, -1.5F, 2.5F, fraction.x, 1)
        texture.Add(CByte(itemSlotBaseTexture + 7))
        For i = 0 To 11
            colours.Add(255)
        Next

        fraction.index = texture.Count
        fraction.x = CSng(1 - (furnaceData.smeltTime / furnaceData.smeltTimeStart))
        If furnaceData.smeltTimeStart = 0 Then fraction.x = 0
        fraction.x = fraction.x * 22 / 16 - 1
        fraction.x = CSng(Math.Floor(fraction.x * 16) / 16)
        If fraction.x < 0 Then fraction.x = 0
        fraction.z = 1
        fractions.Add(fraction)
        DrawBox(coords, -0.5F, 2.5F, fraction.x, 1)
        texture.Add(CByte(itemSlotBaseTexture + 8))
        For i = 0 To 11
            colours.Add(255)
        Next
    End Sub

    Private Shared Sub DrawBox(ByRef coords As List(Of Single), xOffset As Single, zOffset As Single, Optional xSpread As Single = 1, Optional zSpread As Single = 1)
        Dim xChange As Single() = {0, 0, 1, 1}
        Dim zChange As Single() = {0, 1, 1, 0}
        For i = 0 To 3
            xChange(i) *= xSpread
            zChange(i) *= zSpread
        Next
        For j = 0 To 3
            coords.Add(hotbarSize * (xChange(j) + xOffset))
            coords.Add(hotbarSize * (zChange(j) + zOffset))
            coords.Add(0)
            coords.Add(1)
        Next
    End Sub

    Public Shared Sub DisplayCrafting(size As Integer, selected As InventorySelected, ByRef coords As List(Of Single), ByRef texture As List(Of Byte), blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), ByRef colours As List(Of Byte))
        Dim itemSlotBaseTexture As Byte = CByte(RenderWorld.NumTexturesBlocks + RenderWorld.NumTexturesItems + 75)
        Dim xChange As Single() = {0, 0, 1, 1}
        Dim zChange As Single() = {0, 1, 1, 0}

        craftingOutput = CraftingClass.Craft(ExtractBlockId(crafting), itemData)

        For k = 0 To size - 1
            For i = 0 To size - 1
                For j = 0 To 3
                    coords.Add(hotbarSize * (xChange(j) + i - 4.5F))
                    coords.Add(hotbarSize * (zChange(j) - k + 3.5F))
                    coords.Add(0)
                    coords.Add(1)
                Next
                If selected.type = InventoryTypes.Crafting And selected.x = i And selected.y = k Then
                    texture.Add(CByte(itemSlotBaseTexture - 1))
                Else
                    texture.Add(itemSlotBaseTexture)
                End If
                For j = 0 To 11
                    colours.Add(255)
                Next
                DrawIcon(coords, texture, crafting(i, k).itemID, hotbarSize * (i - 4.5F), hotbarSize * (-k + 3.5F), blockData, itemData, crafting(i, k), colours)
                DrawNumber(coords, texture, crafting(i, k).numberOfItems, hotbarSize * (i - 4.5F), hotbarSize * (-k + 3.5F), colours)
            Next
        Next

        For j = 0 To 3
            coords.Add(hotbarSize * (xChange(j) + 0.5F))
            coords.Add(hotbarSize * (zChange(j) + 2.5F))
            coords.Add(0)
            coords.Add(1)
        Next
        If selected.type = InventoryTypes.CraftingOutput Then
            texture.Add(CByte(itemSlotBaseTexture - 1))
        Else
            texture.Add(itemSlotBaseTexture)
        End If
        For i = 0 To 11
            colours.Add(255)
        Next
        DrawIcon(coords, texture, craftingOutput.itemID, hotbarSize * 0.5, hotbarSize * 2.5, blockData, itemData, craftingOutput, colours)
        DrawNumber(coords, texture, craftingOutput.numberOfItems, hotbarSize * 0.5, hotbarSize * 2.5, colours)
    End Sub

    Public Shared Sub DisplayInventory(selected As InventorySelected, ByRef coords As List(Of Single), ByRef texture As List(Of Byte), blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), ByRef colours As List(Of Byte))
        Dim itemSlotBaseTexture As Byte = CByte(RenderWorld.NumTexturesBlocks + RenderWorld.NumTexturesItems + 75)
        Dim xChange As Single() = {0, 0, 1, 1}
        Dim zChange As Single() = {0, 1, 1, 0}
        Dim mousePos As MouseInput.MouseInput
        Dim relMousePos As Single()

        texture = New List(Of Byte)
        coords = New List(Of Single)
        colours = New List(Of Byte)

        mousePos = MouseInput.GetInput()
        relMousePos = MouseInput.GetRelativeLoc(mousePos.x, mousePos.y)
        relMousePos = MouseInput.GetRelativeLocRescaled(relMousePos)

        OpenGL.Clear(198 / 256, 198 / 256, 198 / 256)

        For k = 0 To 2
            For i = 0 To 8
                For j = 0 To 3
                    coords.Add(hotbarSize * (xChange(j) + i - 4.5F))
                    coords.Add(hotbarSize * (zChange(j) - k))
                    coords.Add(0)
                    coords.Add(1)
                Next
                If selected.type = InventoryTypes.Inventory And selected.x = i And selected.y = k Then
                    texture.Add(CByte(itemSlotBaseTexture - 1))
                Else
                    texture.Add(itemSlotBaseTexture)
                End If
                For j = 0 To 11
                    colours.Add(255)
                Next
                DrawIcon(coords, texture, inventory(k * 9 + i).itemID, hotbarSize * (i - 4.5F), hotbarSize * -k, blockData, itemData, inventory(k * 9 + i), colours)
                DrawNumber(coords, texture, inventory(k * 9 + i).numberOfItems, hotbarSize * (i - 4.5F), hotbarSize * -k, colours)
            Next
        Next
        For i = 0 To 8
            For j = 0 To 3
                coords.Add(hotbarSize * (xChange(j) + i - 4.5F))
                coords.Add(hotbarSize * (zChange(j) - 3.5F))
                coords.Add(0)
                coords.Add(1)
            Next
            If selected.type = InventoryTypes.Hotbar And selected.x = i Then
                texture.Add(CByte(itemSlotBaseTexture - 1))
            Else
                texture.Add(itemSlotBaseTexture)
            End If
            For j = 0 To 11
                colours.Add(255)
            Next
            DrawIcon(coords, texture, hotbar(i).itemID, hotbarSize * (i - 4.5F), hotbarSize * -3.5, blockData, itemData, hotbar(i), colours)
            DrawNumber(coords, texture, hotbar(i).numberOfItems, hotbarSize * (i - 4.5F), hotbarSize * -3.5, colours)
        Next

        If holding.itemID = Terrain.Blocks.air Then DisplayCrosshair(texture, coords, relMousePos(0), relMousePos(1), colours)

        DrawBox(coords, -1.5F, 2.5F)
        texture.Add(CByte(itemSlotBaseTexture + 5))
        DrawBox(coords, -0.5F, 2.5F)
        texture.Add(CByte(itemSlotBaseTexture + 6))
        For i = 0 To 23
            colours.Add(255)
        Next
    End Sub

    Public Shared Sub DisplaySelectedBlock(ByRef coords As List(Of Single), ByRef texture As List(Of Byte), blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), ByRef colours As List(Of Byte))
        Dim mousePos As MouseInput.MouseInput
        Dim relMousePos As Single()

        mousePos = MouseInput.GetInput()
        relMousePos = MouseInput.GetRelativeLoc(mousePos.x, mousePos.y)
        relMousePos = MouseInput.GetRelativeLocRescaled(relMousePos)

        DrawIcon(coords, texture, holding.itemID, relMousePos(0) - 0.5F * hotbarSize, relMousePos(1) - 0.5F * hotbarSize, blockData, itemData, holding, colours)
        DrawNumber(coords, texture, holding.numberOfItems, relMousePos(0) - 0.5F * hotbarSize, relMousePos(1) - 0.5F * hotbarSize, colours)
    End Sub

    Public Shared Sub Display(coords As Single(), texture As Byte(), fractions As List(Of OpenGL.FractionalIcon), colours As Byte(), ByRef openglData As OpenGL.OpenGlData)
        OpenGL.InitTextures(texture, RenderWorld.NumTexturesTotal, texture.Length, fractions, openglData)
        'OpenGL.ClearColours(texture.Length * 13)
        OpenGL.RenderGUI(coords, colours, openglData)
    End Sub

    Public Shared Sub DisplayHotbar(blockData As ImportedData.BlockData(), itemData As ImportedData.ItemData(), health As Integer, ByRef openGlData As OpenGL.OpenGlData)
        Dim hotbarBaseTexture As Byte = CByte(RenderWorld.NumTexturesBlocks + RenderWorld.NumTexturesItems + 73)
        Dim hotbarTexture As New List(Of Byte)
        Dim hotbarCoords As New List(Of Single)
        Dim hotbarColours As New List(Of Byte)
        Dim xChange As Single() = {0, 0, 1, 1}
        Dim zChange As Single() = {0, 1, 1, 0}

        For i = 0 To 8
            For j = 0 To 3
                hotbarCoords.Add(hotbarSize * (xChange(j) + i - 4.5F))
                hotbarCoords.Add(-0.8F + hotbarSize * zChange(j))
                hotbarCoords.Add(0)
                hotbarCoords.Add(1)
            Next
            If i = selected Then
                hotbarTexture.Add(CByte(hotbarBaseTexture + 1))
            Else
                hotbarTexture.Add(hotbarBaseTexture)
            End If
            For k = 0 To 11
                hotbarColours.Add(255)
            Next
            DrawIcon(hotbarCoords, hotbarTexture, hotbar(i).itemID, hotbarSize * (i - 4.5F), -0.8, blockData, itemData, hotbar(i), hotbarColours)
            DrawNumber(hotbarCoords, hotbarTexture, hotbar(i).numberOfItems, hotbarSize * (i - 4.5F), -0.8, hotbarColours)
        Next

        For i = 0 To 9
            For j = 0 To 3
                hotbarCoords.Add(hotbarSize * (xChange(j) * heartOffset * heartSize + i * heartOffset - 4.5F))
                hotbarCoords.Add(-0.8F + hotbarSize * (zChange(j) * heartOffset * heartSize + 0.7F))
                hotbarCoords.Add(0)
                hotbarCoords.Add(1)
            Next
            If health - i * 2 = 1 Then
                hotbarTexture.Add(CByte(hotbarBaseTexture + 12))
            ElseIf health <= i * 2 Then
                hotbarTexture.Add(CByte(hotbarBaseTexture + 11))
            Else
                hotbarTexture.Add(CByte(hotbarBaseTexture + 13))
            End If
            For k = 0 To 11
                hotbarColours.Add(255)
            Next
        Next

        DisplayCrosshair(hotbarTexture, hotbarCoords, 0, 0, hotbarColours)

        hotbarTexture.Add(1)

        OpenGL.InitTextures(hotbarTexture.ToArray(), RenderWorld.NumTexturesTotal, hotbarTexture.Count, New List(Of OpenGL.FractionalIcon), openGlData)
        OpenGL.ClearColours(hotbarTexture.Count * 17)
        OpenGL.RenderGUI(hotbarCoords.ToArray(), hotbarColours.ToArray(), openGlData)
    End Sub

    Private Shared Sub DisplayCrosshair(ByRef texture As List(Of Byte), coords As List(Of Single), offsetX As Single, offsetZ As Single, ByRef colours As List(Of Byte))
        Dim xChange As Single() = {0, 0, 1, 1}
        Dim zChange As Single() = {0, 1, 1, 0}
        texture.Add(CByte(RenderWorld.NumTexturesBlocks + RenderWorld.NumTexturesItems + 76))
        For i = 0 To 3
            coords.Add(xChange(i) * crosshairSize - crosshairSize / 2 + offsetX)
            coords.Add(zChange(i) * crosshairSize - crosshairSize / 2 + offsetZ)
            coords.Add(0)
            coords.Add(1)
        Next
        For i = 0 To 11
            colours.Add(255)
        Next
    End Sub

    Public Structure InventoryItem
        Public itemID As Byte
        Public numberOfItems As Byte
        Public durability As Integer
    End Structure

    Public Structure InventorySelected
        Public type As InventoryTypes
        Public x As Integer
        Public y As Integer
    End Structure

    Public Enum InventoryTypes
        NoneSelected = -1
        Hotbar = 0
        Inventory = 1
        Crafting = 2
        CraftingOutput = 3
        FurnaceOutput = 4
        FurnaceFuel = 5
        FurnaceSmelt = 6
    End Enum
End Class

Public Class FEN
    Public Structure BlockChange
        Public blockID As Byte
        Public coord As Integer
    End Structure

    Public Structure ChunkChanges
        Public changes As List(Of BlockChange)
        Public coord As RenderWorld.ChunkCoord
        Public inUse As Boolean
    End Structure

    Public Shared Sub EditBlock(block As BlockChange, ByRef allChanges As List(Of BlockChange))
        Dim i As Integer = 0
        i = 0
        While i < allChanges.Count
            If allChanges(i).coord = block.coord Then
                allChanges.RemoveAt(i)
            Else
                i += 1
            End If
        End While
        allChanges.Add(block)
    End Sub

    Public Shared Function GetChangesFromChunk(coord As RenderWorld.ChunkCoord, ByRef allChanges As ChunkChanges(), Optional toSet As Boolean = False) As List(Of BlockChange)
        For i = 0 To allChanges.Length - 1
            If coord.x = allChanges(i).coord.x And coord.z = allChanges(i).coord.z And allChanges(i).inUse Then
                Return allChanges(i).changes
            End If
        Next
        If toSet Then
            AddNewChunk(coord, allChanges)
            For i = 0 To allChanges.Length - 1
                If coord.x = allChanges(i).coord.x And coord.z = allChanges(i).coord.z And allChanges(i).inUse Then
                    Return allChanges(i).changes
                End If
            Next
        End If
        Return New List(Of BlockChange)
    End Function

    Private Shared Sub InputDataPlayer(data As String())
        Dim splitData As String()

        splitData = data(1).Split(","c)
        Player.x = CInt(splitData(0)) - CInt((CSng(splitData(0)) - Player.x) / 16)
        Player.chunkX = CInt((CSng(splitData(0)) - Player.x) / 16)
        Player.y = CInt(splitData(1))
        Player.z = CInt(splitData(2)) - CInt((CSng(splitData(2)) - Player.z) / 16) * 16
        Player.chunkZ = CInt((CSng(splitData(2)) - Player.z) / 16)
        Player.health = CInt(splitData(3))

        splitData = data(2).Split(";"c)
        For i = 0 To 8
            Inventory.hotbar(i) = ExtractInventoryItem(splitData(i))
        Next
        For i = 9 To 35
            Inventory.inventory(i - 9) = ExtractInventoryItem(splitData(i))
        Next
    End Sub

    Private Shared Function ExtractInventoryItem(data As String) As Inventory.InventoryItem
        Dim inventoryItem As New Inventory.InventoryItem
        Dim durabilityCatch As Integer = CInt(data.Split(":"c)(2))
        If durabilityCatch < 0 Then durabilityCatch = 0
        inventoryItem.itemID = CByte(data.Split(":"c)(0))
        inventoryItem.numberOfItems = CByte(data.Split(":"c)(1))
        inventoryItem.durability = CByte(durabilityCatch)
        Return inventoryItem
    End Function

    Private Shared Function InputDataFurnace(dataLine As String) As Furnace()
        Dim splitData As String() = dataLine.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim data(splitData.Length - 1) As Furnace
        For i = 0 To data.Length - 1
            data(i) = New Furnace()
            data(i).InputData(splitData(i))
        Next
        Return data
    End Function

    Public Shared Sub InputData(fileName As String, ByRef allChunkChanges As ChunkChanges(), ByRef seed As Integer, ByRef torchData As List(Of RenderWorld.TorchData), ByRef furnace As Furnace())
        Dim data As String() = IO.File.ReadAllLines("SavedGames\" & fileName & ".txt")
        Dim splitData As String()
        Dim splitSplitData As String()
        Dim splitSplitDataA As String()
        Dim blockChange As New BlockChange
        Dim newTorchData As New RenderWorld.TorchData(True)
        Dim chunkX, chunkZ As Integer
        Const WORLDDATASTART = 4
        seed = CInt(data(0))

        InputDataPlayer(data)
        furnace = InputDataFurnace(data(3))

        ReDim allChunkChanges(data.Length)
        For i = WORLDDATASTART To data.Length - 1
            splitData = data(i).Split({"!"c}, StringSplitOptions.None)
            allChunkChanges(i - WORLDDATASTART).inUse = True
            allChunkChanges(i - WORLDDATASTART).coord = New RenderWorld.ChunkCoord()
            chunkX = CInt(splitData(0).Split(","c)(0))
            chunkZ = CInt(splitData(0).Split(","c)(1))
            allChunkChanges(i - WORLDDATASTART).coord.x = chunkX
            allChunkChanges(i - WORLDDATASTART).coord.z = chunkZ
            allChunkChanges(i - WORLDDATASTART).changes = New List(Of BlockChange)

            splitSplitData = splitData(1).Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
            splitSplitDataA = splitData(2).Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
            For j = 0 To splitSplitData.Length - 1
                newTorchData.chunk.x = chunkX
                newTorchData.chunk.z = chunkZ
                newTorchData.location = CInt(splitSplitData(j))
                newTorchData.orientation = CType(splitSplitDataA(j), CustomBlocks.TorchBlockAttached)
                RenderWorld.AllTorchData.Add(newTorchData)
            Next

            splitSplitData = splitData(3).Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
            For j = 0 To splitSplitData.Length - 1
                blockChange.coord = CInt(splitSplitData(j).Split(":"c)(0))
                blockChange.blockID = CByte(splitSplitData(j).Split(":"c)(1))
                allChunkChanges(i - WORLDDATASTART).changes.Add(blockChange)
            Next
        Next
    End Sub

    Private Shared Function InventoryData() As String
        Dim data As New Text.StringBuilder
        For i = 0 To 8
            data.Append(Inventory.hotbar(i).itemID)
            data.Append(":")
            data.Append(Inventory.hotbar(i).numberOfItems)
            data.Append(":")
            data.Append(Inventory.hotbar(i).durability)
            data.Append(";")
        Next
        For i = 0 To 26
            data.Append(Inventory.inventory(i).itemID)
            data.Append(":")
            data.Append(Inventory.inventory(i).numberOfItems)
            data.Append(":")
            data.Append(Inventory.inventory(i).durability)
            data.Append(";")
        Next
        Return data.ToString
    End Function

    Private Shared Function GetLocations(ByRef torchData As List(Of RenderWorld.TorchData), chunkX As Integer, chunkZ As Integer) As List(Of Integer)
        Dim locations As New List(Of Integer)
        For i = 0 To torchData.Count - 1
            If torchData(i).chunk.x = chunkX AndAlso torchData(i).chunk.z = chunkZ Then
                locations.Add(torchData(i).location)
            End If
        Next
        Return locations
    End Function

    Private Shared Function GetOrientations(ByRef torchData As List(Of RenderWorld.TorchData), chunkX As Integer, chunkZ As Integer) As List(Of CustomBlocks.TorchBlockAttached)
        Dim orientations As New List(Of CustomBlocks.TorchBlockAttached)
        For i = 0 To torchData.Count - 1
            If torchData(i).chunk.x = chunkX AndAlso torchData(i).chunk.z = chunkZ Then
                orientations.Add(torchData(i).orientation)
            End If
        Next
        Return orientations
    End Function

    Private Shared Function TorchData(locations As List(Of Integer), orientations As List(Of CustomBlocks.TorchBlockAttached)) As String
        Dim toOutput As New Text.StringBuilder
        For i = 0 To locations.Count - 1
            toOutput.Append(locations(i) & ";")
        Next
        toOutput.Append("!")
        For i = 0 To orientations.Count - 1
            toOutput.Append(orientations(i) & ";")
        Next
        toOutput.Append("!")
        Return toOutput.ToString()
    End Function

    Private Shared Function FurnaceData(ByRef furnaces As Furnace()) As String
        Dim data As New Text.StringBuilder
        For i = 0 To furnaces.Length - 1
            If Not IsNothing(furnaces(i)) Then
                data.Append(furnaces(i).OutputData)
                data.Append(";")
            End If
        Next
        Return data.ToString
    End Function

    Public Shared Sub OutputData(ByRef allChunkChanges As ChunkChanges(), seed As Integer, ByRef allTorches As List(Of RenderWorld.TorchData), saveGameName As String, ByRef furnaces As Furnace())
        Dim toOutput As New Text.StringBuilder
        Dim chunkX, chunkZ As Integer
        toOutput.AppendLine(seed.ToString())
        toOutput.Append((Player.x + Player.chunkX * 16).ToString() & ",")
        toOutput.Append(Player.y.ToString() & ",")
        toOutput.Append((Player.z + Player.chunkZ * 16).ToString() & ",")
        toOutput.Append(Player.health)
        toOutput.AppendLine()
        toOutput.AppendLine(InventoryData)
        toOutput.AppendLine(FurnaceData(furnaces))
        For i = 0 To allChunkChanges.Length - 1
            If Not IsNothing(allChunkChanges(i).changes) And allChunkChanges(i).inUse Then
                chunkX = allChunkChanges(i).coord.x
                chunkZ = allChunkChanges(i).coord.z
                toOutput.Append(chunkX & "," & chunkZ)
                toOutput.Append("!")
                toOutput.Append(TorchData(GetLocations(allTorches, chunkX, chunkZ), GetOrientations(allTorches, chunkX, chunkZ)))
                For j = 0 To allChunkChanges(i).changes.Count - 1
                    toOutput.Append(allChunkChanges(i).changes(j).coord)
                    toOutput.Append(":")
                    toOutput.Append(allChunkChanges(i).changes(j).blockID)
                    toOutput.Append(";")
                Next
                toOutput.AppendLine()
            End If
        Next
        IO.File.WriteAllText("SavedGames\" & saveGameName & ".txt", toOutput.ToString())
    End Sub

    Public Shared Sub AddNewChunk(coord As RenderWorld.ChunkCoord, ByRef allChanges As ChunkChanges())
        Dim indexNextFree As Integer = -1
        For i = 0 To allChanges.Length - 1
            If allChanges(i).inUse Then
                If allChanges(i).coord.x = coord.x And allChanges(i).coord.z = coord.z Then
                    allChanges(i).inUse = True
                    Return
                End If
            Else
                indexNextFree = i
            End If
        Next
        If indexNextFree = -1 Then
            indexNextFree = allChanges.Length
            ReDim Preserve allChanges(allChanges.Length * 2 + 1)
        End If
        allChanges(indexNextFree).inUse = True
        allChanges(indexNextFree).coord.x = coord.x
        allChanges(indexNextFree).coord.z = coord.z
        allChanges(indexNextFree).changes = New List(Of BlockChange)
    End Sub
End Class