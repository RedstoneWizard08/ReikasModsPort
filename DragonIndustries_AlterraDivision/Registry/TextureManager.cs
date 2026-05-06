using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nautilus.Utility;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class TextureManager {
    private static readonly Dictionary<Assembly, Dictionary<string, Texture2D>> textures = new();

    private static readonly Dictionary<Assembly, Dictionary<string, Sprite>> sprites = new();

    private static readonly HashSet<string> notFound = [];

    static TextureManager() {
    }

    public static void refresh() {
        textures.Clear();
    }

    public static Texture2D getTexture(Assembly a, string path) {
        if (a == null)
            throw new Exception("You must specify a mod to load the texture for!");
        if (!textures.ContainsKey(a))
            textures[a] = new Dictionary<string, Texture2D>();
        if (!textures[a].ContainsKey(path)) {
            textures[a][path] = loadTexture(a, path, out var found);
            if (!found)
                notFound.Add(path);
        }

        return textures[a][path];
    }

    private static Texture2D loadTexture(Assembly a, string relative, out bool found) {
        var folder = Path.GetDirectoryName(a.Location);
        var path = Path.Combine(folder, relative + ".png");
        SNUtil.Log("Loading texture from '" + path + "'", a);
        var newTex = ImageUtils.LoadTextureFromFile(path);
        found = File.Exists(path);
        return newTex;
    }

    public static Sprite getSprite(Assembly a, string path) {
        if (a == null)
            throw new Exception("You must specify a mod to load the texture for!");
        if (!sprites.ContainsKey(a))
            sprites[a] = new Dictionary<string, Sprite>();
        if (!sprites[a].ContainsKey(path)) {
            sprites[a][path] = loadSprite(a, path, out var found);
            if (!found)
                notFound.Add(path);
        }

        return sprites[a][path];
    }

    public static Sprite createSprite(Texture2D tex, int pxPerUnit = 512) {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), pxPerUnit);
    }

    private static Sprite loadSprite(Assembly a, string relative, out bool found) {
        var folder = Path.GetDirectoryName(a.Location);
        var path = Path.Combine(folder, relative + ".png");
        SNUtil.Log("Loading sprite from '" + path + "'", a);

        if (!File.Exists(path)) {
            found = false;
            return null;
        }
        
        var newTex = ImageUtils.LoadSpriteFromFile(path);
        found = File.Exists(path);
        return newTex;
    }

    public static bool isTextureNotFound(string path) {
        return notFound.Contains(path);
    }
}