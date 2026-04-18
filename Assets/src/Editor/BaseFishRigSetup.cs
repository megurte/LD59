using System;
using System.Collections.Generic;
using System.Linq;
using Core.Fish;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;

namespace Editor
{
    public static class BaseFishRigSetup
    {
        private const string SpritePath = "Assets/Resources/Sprites/basefish.psd";
        private const string PrefabPath = "Assets/Resources/CMS/BaseFish.prefab";
        private const string SpriteName = "fullfish";

        private static bool _isRunning;

        [MenuItem("Tools/Fish/Setup BaseFish Rig")]
        public static void SetupFromMenu()
        {
            TrySetup();
        }

        private static void TrySetup()
        {
            if (_isRunning || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            _isRunning = true;

            try
            {
                if (!SetupSpriteRig())
                {
                    return;
                }

                SetupPrefabRig();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                _isRunning = false;
            }
        }

        private static bool SetupSpriteRig()
        {
            var importer = AssetImporter.GetAtPath(SpritePath);
            if (importer == null)
            {
                return false;
            }

            var factories = new SpriteDataProviderFactories();
            factories.Init();

            var dataProvider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider == null)
            {
                return false;
            }

            dataProvider.InitSpriteEditorDataProvider();

            var spriteRect = dataProvider.GetSpriteRects().FirstOrDefault(rect => rect.name == SpriteName);
            if (spriteRect == null)
            {
                return false;
            }

            var width = spriteRect.rect.width;
            var height = spriteRect.rect.height;
            var pixelsPerUnit = dataProvider.pixelsPerUnit;
            var bones = CreateBones(width, height, pixelsPerUnit);
            var vertices = CreateVertices(width, height, pixelsPerUnit);
            var indices = CreateIndices(7, 15);
            var edges = CreateEdges(7, 15);

            var boneDataProvider = dataProvider.GetDataProvider<ISpriteBoneDataProvider>();
            var meshDataProvider = dataProvider.GetDataProvider<ISpriteMeshDataProvider>();
            if (boneDataProvider == null || meshDataProvider == null)
            {
                return false;
            }

            boneDataProvider.SetBones(spriteRect.spriteID, bones);
            meshDataProvider.SetVertices(spriteRect.spriteID, vertices);
            meshDataProvider.SetIndices(spriteRect.spriteID, indices);
            meshDataProvider.SetEdges(spriteRect.spriteID, edges);

            dataProvider.Apply();
            importer.SaveAndReimport();
            return true;
        }

        private static void SetupPrefabRig()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

            try
            {
                var movement = prefabRoot.GetComponent<FishMovementController>();
                var fishAnimator = prefabRoot.GetComponent<FishAnimator>();
                if (fishAnimator != null)
                {
                    UnityEngine.Object.DestroyImmediate(fishAnimator, true);
                }

                var boneAnimator = prefabRoot.GetComponent<FishBoneAnimator>();
                if (boneAnimator == null)
                {
                    boneAnimator = prefabRoot.AddComponent<FishBoneAnimator>();
                }

                var body = prefabRoot.GetComponentsInChildren<Transform>(true).First(transform => transform.name == "body");
                var bodyRenderer = body.GetComponent<SpriteRenderer>();
                var spriteSkin = body.GetComponent<SpriteSkin>() ?? body.gameObject.AddComponent<SpriteSkin>();
                var sprite = LoadFullFishSprite();

                if (sprite != null)
                {
                    bodyRenderer.sprite = sprite;
                }

                var spriteMaterial = prefabRoot
                    .GetComponentsInChildren<SpriteRenderer>(true)
                    .FirstOrDefault(renderer => renderer != bodyRenderer && renderer.sharedMaterial != null)
                    ?.sharedMaterial;

                if (spriteMaterial != null)
                {
                    bodyRenderer.sharedMaterial = spriteMaterial;
                }

                foreach (var renderer in prefabRoot.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    renderer.enabled = renderer == bodyRenderer;
                }

                var rigRoot = body.GetComponentsInChildren<Transform>(true).FirstOrDefault(transform => transform.name == "Rig");
                if (rigRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(rigRoot.gameObject);
                }

                rigRoot = new GameObject("Rig").transform;
                rigRoot.SetParent(body, false);

                var head = CreateBoneTransform(rigRoot, "head", new Vector3(0f, 0.98f, 0f));
                var spine01 = CreateBoneTransform(head, "spine_01", new Vector3(0f, -0.33f, 0f));
                var spine02 = CreateBoneTransform(spine01, "spine_02", new Vector3(0f, -0.37f, 0f));
                var spine03 = CreateBoneTransform(spine02, "spine_03", new Vector3(0f, -0.36f, 0f));
                var spine04 = CreateBoneTransform(spine03, "spine_04", new Vector3(0f, -0.4f, 0f));
                var tail01 = CreateBoneTransform(spine04, "tail_01", new Vector3(0f, -0.34f, 0f));
                var tail02 = CreateBoneTransform(tail01, "tail_02", new Vector3(0f, -0.26f, 0f));
                var leftFin01 = CreateBoneTransform(spine02, "left_fin_01", new Vector3(-0.3f, 0.12f, 0f));
                var rightFin01 = CreateBoneTransform(spine02, "right_fin_01", new Vector3(0.3f, 0.12f, 0f));
                var leftFin02 = CreateBoneTransform(spine03, "left_fin_02", new Vector3(-0.24f, -0.1f, 0f));
                var rightFin02 = CreateBoneTransform(spine03, "right_fin_02", new Vector3(0.24f, -0.1f, 0f));

                var boneTransforms = new[]
                {
                    head,
                    spine01,
                    spine02,
                    spine03,
                    spine04,
                    tail01,
                    tail02,
                    leftFin01,
                    rightFin01,
                    leftFin02,
                    rightFin02
                };

                spriteSkin.alwaysUpdate = true;
                spriteSkin.autoRebind = false;
                spriteSkin.SetRootBone(head);
                spriteSkin.SetBoneTransforms(boneTransforms);
                spriteSkin.ResetBindPose();
                SetBounds(spriteSkin, bodyRenderer);

                ConfigureBoneAnimator(
                    boneAnimator,
                    movement,
                    body,
                    new[] { head, spine01, spine02, spine03, spine04, tail01, tail02 },
                    new[] { leftFin01, leftFin02 },
                    new[] { rightFin01, rightFin02 });

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ConfigureBoneAnimator(
            FishBoneAnimator animator,
            FishMovementController movement,
            Transform rootVisual,
            Transform[] spineBones,
            Transform[] leftFinBones,
            Transform[] rightFinBones)
        {
            var serializedObject = new SerializedObject(animator);

            serializedObject.FindProperty("movement").objectReferenceValue = movement;
            serializedObject.FindProperty("rootVisual").objectReferenceValue = rootVisual;
            SetTransformArray(serializedObject.FindProperty("spineBones"), spineBones);
            SetTransformArray(serializedObject.FindProperty("leftFinBones"), leftFinBones);
            SetTransformArray(serializedObject.FindProperty("rightFinBones"), rightFinBones);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetTransformArray(SerializedProperty property, Transform[] values)
        {
            property.arraySize = values.Length;

            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetBounds(SpriteSkin spriteSkin, SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer.sprite == null)
            {
                return;
            }

            var size = spriteRenderer.sprite.bounds.size;
            var bounds = new Bounds(Vector3.zero, new Vector3(size.x + 0.4f, size.y + 0.4f, 1f));
            var serializedObject = new SerializedObject(spriteSkin);
            serializedObject.FindProperty("m_Bounds").boundsValue = bounds;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform CreateBoneTransform(Transform parent, string name, Vector3 localPosition)
        {
            var bone = new GameObject(name).transform;
            bone.SetParent(parent, false);
            bone.localPosition = localPosition;
            bone.localRotation = Quaternion.identity;
            bone.localScale = Vector3.one;
            return bone;
        }

        private static List<SpriteBone> CreateBones(float width, float height, float pixelsPerUnit)
        {
            var definitions = new[]
            {
                new BoneDefinition("head", -1, new Vector3(0f, 0.98f, 0f), 0.33f),
                new BoneDefinition("spine_01", 0, new Vector3(0f, -0.33f, 0f), 0.37f),
                new BoneDefinition("spine_02", 1, new Vector3(0f, -0.37f, 0f), 0.36f),
                new BoneDefinition("spine_03", 2, new Vector3(0f, -0.36f, 0f), 0.4f),
                new BoneDefinition("spine_04", 3, new Vector3(0f, -0.4f, 0f), 0.34f),
                new BoneDefinition("tail_01", 4, new Vector3(0f, -0.34f, 0f), 0.26f),
                new BoneDefinition("tail_02", 5, new Vector3(0f, -0.26f, 0f), 0.18f),
                new BoneDefinition("left_fin_01", 2, new Vector3(-0.3f, 0.12f, 0f), 0.26f),
                new BoneDefinition("right_fin_01", 2, new Vector3(0.3f, 0.12f, 0f), 0.26f),
                new BoneDefinition("left_fin_02", 3, new Vector3(-0.24f, -0.1f, 0f), 0.2f),
                new BoneDefinition("right_fin_02", 3, new Vector3(0.24f, -0.1f, 0f), 0.2f)
            };

            var bones = new List<SpriteBone>(definitions.Length);

            foreach (var definition in definitions)
            {
                var position = definition.ParentId == -1
                    ? new Vector3(width * 0.5f + definition.Position.x * pixelsPerUnit, height * 0.5f + definition.Position.y * pixelsPerUnit, 0f)
                    : definition.Position * pixelsPerUnit;

                bones.Add(new SpriteBone
                {
                    name = definition.Name,
                    parentId = definition.ParentId,
                    position = position,
                    rotation = Quaternion.identity,
                    length = definition.Length * pixelsPerUnit,
                    guid = GUID.Generate().ToString(),
                    color = new Color(0.32f, 0.78f, 0.9f, 1f)
                });
            }

            return bones;
        }

        private static Vertex2DMetaData[] CreateVertices(float width, float height, float pixelsPerUnit)
        {
            const int columns = 7;
            const int rows = 15;

            var vertices = new Vertex2DMetaData[columns * rows];
            var unitWidth = width / pixelsPerUnit;
            var unitHeight = height / pixelsPerUnit;
            var top = unitHeight * 0.5f;
            var bottom = -top;

            for (var row = 0; row < rows; row++)
            {
                var t = rows == 1 ? 0f : row / (float)(rows - 1);
                var y = Mathf.Lerp(top, bottom, t);
                var halfWidth = GetHalfWidth(unitWidth, t);

                for (var column = 0; column < columns; column++)
                {
                    var x01 = columns == 1 ? 0.5f : column / (float)(columns - 1);
                    var x = Mathf.Lerp(-halfWidth, halfWidth, x01);
                    var index = row * columns + column;

                    vertices[index] = new Vertex2DMetaData
                    {
                        position = new Vector2(width * 0.5f + x * pixelsPerUnit, height * 0.5f + y * pixelsPerUnit),
                        boneWeight = CreateBoneWeight(new Vector2(x, y), unitWidth, unitHeight, t)
                    };
                }
            }

            return vertices;
        }

        private static float GetHalfWidth(float width, float t)
        {
            var bodyWidth = 0.18f + Mathf.Sin(t * Mathf.PI) * 0.17f;
            var upperFinWidth = Band(t, 0.34f, 0.12f) * 0.14f;
            var lowerFinWidth = Band(t, 0.6f, 0.08f) * 0.1f;
            var tailTaper = Mathf.Lerp(1f, 0.42f, Mathf.Clamp01((t - 0.7f) / 0.3f));
            return width * (bodyWidth * tailTaper + upperFinWidth + lowerFinWidth);
        }

        private static BoneWeight CreateBoneWeight(Vector2 position, float width, float height, float t)
        {
            var weights = new List<BoneInfluence>(8);
            var spinePosition = t * 6f;
            var spineIndex = Mathf.Clamp(Mathf.FloorToInt(spinePosition), 0, 6);
            var nextSpineIndex = Mathf.Min(spineIndex + 1, 6);
            var spineBlend = spinePosition - spineIndex;

            AddWeight(weights, spineIndex, 1f - spineBlend);
            AddWeight(weights, nextSpineIndex, spineBlend);

            var headRigidity = Mathf.Clamp01((position.y - height * 0.08f) / (height * 0.34f));
            AddWeight(weights, 0, headRigidity * 1.6f);

            var tailBias = Mathf.Clamp01((-position.y - height * 0.08f) / (height * 0.7f));
            AddWeight(weights, 5, tailBias * 0.45f);
            AddWeight(weights, 6, tailBias * 0.8f);

            var lateral = Mathf.Clamp01((Mathf.Abs(position.x) - width * 0.1f) / (width * 0.3f));
            var upperFin = Band(t, 0.34f, 0.11f) * lateral;
            var lowerFin = Band(t, 0.6f, 0.08f) * lateral;

            if (position.x < 0f)
            {
                AddWeight(weights, 7, upperFin * 0.9f);
                AddWeight(weights, 9, lowerFin * 0.75f);
            }
            else if (position.x > 0f)
            {
                AddWeight(weights, 8, upperFin * 0.9f);
                AddWeight(weights, 10, lowerFin * 0.75f);
            }

            return NormalizeBoneWeight(weights);
        }

        private static BoneWeight NormalizeBoneWeight(List<BoneInfluence> weights)
        {
            if (weights.Count == 0)
            {
                return new BoneWeight
                {
                    boneIndex0 = 0,
                    weight0 = 1f
                };
            }

            weights.Sort((left, right) => right.Weight.CompareTo(left.Weight));

            var count = Mathf.Min(4, weights.Count);
            var total = 0f;

            for (var i = 0; i < count; i++)
            {
                total += weights[i].Weight;
            }

            if (total <= 0f)
            {
                return new BoneWeight
                {
                    boneIndex0 = 0,
                    weight0 = 1f
                };
            }

            var boneWeight = new BoneWeight();

            if (count > 0)
            {
                boneWeight.boneIndex0 = weights[0].BoneIndex;
                boneWeight.weight0 = weights[0].Weight / total;
            }

            if (count > 1)
            {
                boneWeight.boneIndex1 = weights[1].BoneIndex;
                boneWeight.weight1 = weights[1].Weight / total;
            }

            if (count > 2)
            {
                boneWeight.boneIndex2 = weights[2].BoneIndex;
                boneWeight.weight2 = weights[2].Weight / total;
            }

            if (count > 3)
            {
                boneWeight.boneIndex3 = weights[3].BoneIndex;
                boneWeight.weight3 = weights[3].Weight / total;
            }

            return boneWeight;
        }

        private static void AddWeight(List<BoneInfluence> weights, int boneIndex, float weight)
        {
            if (weight <= 0f)
            {
                return;
            }

            for (var i = 0; i < weights.Count; i++)
            {
                if (weights[i].BoneIndex != boneIndex)
                {
                    continue;
                }

                weights[i] = new BoneInfluence(boneIndex, weights[i].Weight + weight);
                return;
            }

            weights.Add(new BoneInfluence(boneIndex, weight));
        }

        private static int[] CreateIndices(int columns, int rows)
        {
            var indices = new List<int>((columns - 1) * (rows - 1) * 6);

            for (var row = 0; row < rows - 1; row++)
            {
                for (var column = 0; column < columns - 1; column++)
                {
                    var topLeft = row * columns + column;
                    var topRight = topLeft + 1;
                    var bottomLeft = topLeft + columns;
                    var bottomRight = bottomLeft + 1;

                    indices.Add(topLeft);
                    indices.Add(bottomLeft);
                    indices.Add(topRight);

                    indices.Add(topRight);
                    indices.Add(bottomLeft);
                    indices.Add(bottomRight);
                }
            }

            return indices.ToArray();
        }

        private static Vector2Int[] CreateEdges(int columns, int rows)
        {
            var edges = new List<Vector2Int>((columns - 1) * rows + (rows - 1) * columns);

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns - 1; column++)
                {
                    var index = row * columns + column;
                    edges.Add(new Vector2Int(index, index + 1));
                }
            }

            for (var column = 0; column < columns; column++)
            {
                for (var row = 0; row < rows - 1; row++)
                {
                    var index = row * columns + column;
                    edges.Add(new Vector2Int(index, index + columns));
                }
            }

            return edges.ToArray();
        }

        private static float Band(float value, float center, float radius)
        {
            if (radius <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(1f - Mathf.Abs(value - center) / radius);
        }

        private static Sprite LoadFullFishSprite()
        {
            return AssetDatabase.LoadAllAssetsAtPath(SpritePath)
                .OfType<Sprite>()
                .FirstOrDefault(sprite => sprite.name == SpriteName);
        }

        private readonly struct BoneDefinition
        {
            public readonly string Name;
            public readonly int ParentId;
            public readonly Vector3 Position;
            public readonly float Length;

            public BoneDefinition(string name, int parentId, Vector3 position, float length)
            {
                Name = name;
                ParentId = parentId;
                Position = position;
                Length = length;
            }
        }

        private readonly struct BoneInfluence
        {
            public readonly int BoneIndex;
            public readonly float Weight;

            public BoneInfluence(int boneIndex, float weight)
            {
                BoneIndex = boneIndex;
                Weight = weight;
            }
        }
    }
}
