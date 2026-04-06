# Project Standards: PartTimeCat

This project is a Unity-based game using VContainer for DI and Netcode for GameObjects for networking.

## General Principles
- **Clarity over Brevity:** Ensure variables, methods, and classes are descriptively named.
- **Dependency Injection:** Use VContainer. Prefer constructor injection for plain C# classes and `[Inject]` for MonoBehaviours where necessary, but ideally register components in a `LifetimeScope`.
- **Decoupling:** Use `MessagePipe` for inter-component communication to keep systems decoupled.

## Directory & File Structure
- Root Assets directory uses an underscore prefix for major organizational folders:
    - `_Scripts`: All C# scripts.
    - `_Prefabs`: Reusable Unity prefabs.
    - `_Models`: 3D models and FBX files.
    - `_Anims`: Animation clips and controllers.
    - `_SO`: ScriptableObject instances.
    - `_Scenes`: Unity scene files.
- Subdirectories within `_Scripts` should correspond to namespaces.

## Coding Standards (C#)
- **Namespaces:** Always wrap scripts in a namespace starting with `_Scripts`. Follow the directory structure (e.g., `namespace _Scripts._Shared.UI`).
- **Naming Conventions:**
    - Classes/Structs: `PascalCase`
    - Interfaces: `IPascalCase`
    - Methods: `PascalCase`
    - Private Fields: `camelCase` (no underscore prefix, e.g., `private int itemCount;`)
    - Public Fields/Properties: `PascalCase`
    - Constants/Statics: `PascalCase` or `UPPER_SNAKE_CASE` (follow existing patterns).
- **Unity Attributes:** Use the alias `[SF]` for `[SerializeField]`. It is often defined as `using SF = UnityEngine.SerializeField;` at the top of scripts.
- **Formatting:**
    - Braces on new lines.
    - 4 spaces for indentation.
- **Async/Await:** Use `UniTask` if available (check project dependencies) or standard `Task` for asynchronous operations.

## Architecture Patterns
- **LifetimeScopes:** Manage dependencies within `LifetimeScope` components (e.g., `RootScope.cs`).
- **Data Handling:** Use ScriptableObjects for configuration and static data (stored in `_SO`).
- **Networking:** Use `Unity.Netcode` (NetworkManager, NetworkBehaviour, etc.).

## Commit Guidelines
- Use descriptive commit messages.
- Prefix commits with the area of change if applicable (e.g., `feat(ui): ...`, `fix(net): ...`).
