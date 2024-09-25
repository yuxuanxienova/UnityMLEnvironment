# Unity ML Environment

## Project Architecture

### ./
``` bash
├───Assets
│   ├───Materials
│   │   └───Textures
│   ├───Meshes
│   ├───Prefab
│   ├───PythonCode
│   │   ├───NetworkServer
│   │   │       
│   │   └───Trainner # Training code here 
│   ├───Scenes
│   ├───Scripts
│   │   ├───Algo
│   │   ├───NetworkClient
│   │   ├───RLAgent  # Agent Scripts here
│   │   └───Utilities
```

## Start trainning
1. Open Current folder as Unity project
2. load SampleScene from ./Assets/Scenes/
3. run ./Assets/PythonCode/Trainner/run_trainner.py
4. Click "Play" in unity 