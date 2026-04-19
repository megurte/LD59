using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Fish;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;

namespace Editor
{
    public static class SharkRigSetup
    {
        private const string SpritePath = "Assets/Resources/CMS/Sprites/sharkEnemy.psd";
        private const string PrefabPath = "Assets/Resources/CMS/Shark.prefab";
        private const string SpriteName = "sharkEnemy_0";
        private const int HeadBone = 0;
        private const int LeftHeadBone = 1;
        private const int RightHeadBone = 2;
        private const int Spine01Bone = 3;
        private const int Spine02Bone = 4;
        private const int Spine03Bone = 5;
        private const int Spine04Bone = 6;
        private const int Tail01Bone = 7;
        private const int Tail02Bone = 8;
        private const int LeftSupportBone = 9;
        private const int RightSupportBone = 10;
        private const int LeftFin01Bone = 11;
        private const int RightFin01Bone = 12;
        private const int LeftFin02Bone = 13;
        private const int RightFin02Bone = 14;

        private static readonly BoneDefinition[] BoneDefinitions =
        {
            new("head", -1, new Vector3(0f, 3.08f, 0f), 0.8f),
            new("left_head", HeadBone, new Vector3(-1.06f, 0.12f, 0f), 0.46f),
            new("right_head", HeadBone, new Vector3(1.06f, 0.12f, 0f), 0.46f),
            new("spine_01", HeadBone, new Vector3(0f, -0.95f, 0f), 0.82f),
            new("spine_02", Spine01Bone, new Vector3(0f, -0.82f, 0f), 0.84f),
            new("spine_03", Spine02Bone, new Vector3(0f, -0.84f, 0f), 0.92f),
            new("spine_04", Spine03Bone, new Vector3(0f, -0.92f, 0f), 0.98f),
            new("tail_01", Spine04Bone, new Vector3(0f, -0.98f, 0f), 1.02f),
            new("tail_02", Tail01Bone, new Vector3(0f, -1.02f, 0f), 0.84f),
            new("left_support", Spine02Bone, new Vector3(-0.74f, -0.12f, 0f), 0.42f),
            new("right_support", Spine02Bone, new Vector3(0.74f, -0.12f, 0f), 0.42f),
            new("left_fin_01", LeftSupportBone, new Vector3(-0.38f, -0.02f, 0f), 0.74f),
            new("right_fin_01", RightSupportBone, new Vector3(0.38f, -0.02f, 0f), 0.74f),
            new("left_fin_02", Spine03Bone, new Vector3(-0.54f, -0.46f, 0f), 0.38f),
            new("right_fin_02", Spine03Bone, new Vector3(0.54f, -0.46f, 0f), 0.38f)
        };

        private static bool _isRunning;

        [MenuItem("Tools/Fish/Setup Shark Rig")]
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

            var spriteRects = dataProvider.GetSpriteRects();
            var spriteRect = spriteRects.FirstOrDefault(rect => rect.name == SpriteName);
            if (spriteRect == null)
            {
                return false;
            }

            spriteRect.alignment = SpriteAlignment.Center;
            spriteRect.pivot = Vector2.one * 0.5f;
            var width = spriteRect.rect.width;
            var height = spriteRect.rect.height;

            dataProvider.SetSpriteRects(spriteRects);

            var pixelsPerUnit = dataProvider.pixelsPerUnit;

            var boneDataProvider = dataProvider.GetDataProvider<ISpriteBoneDataProvider>();
            var meshDataProvider = dataProvider.GetDataProvider<ISpriteMeshDataProvider>();
            if (boneDataProvider == null || meshDataProvider == null)
            {
                return false;
            }

            boneDataProvider.SetBones(spriteRect.spriteID, CreateBones(width, height, pixelsPerUnit));
            meshDataProvider.SetVertices(spriteRect.spriteID, CreateVertices(width, height, pixelsPerUnit));
            meshDataProvider.SetIndices(spriteRect.spriteID, CreateIndices(9, 21));
            meshDataProvider.SetEdges(spriteRect.spriteID, CreateEdges(9, 21));

            dataProvider.Apply();
            importer.SaveAndReimport();
            CleanupMultipleMeta();
            return true;
        }

        private static void SetupPrefabRig()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

            try
            {
                var movement = prefabRoot.GetComponent<FishMovementController>() ?? prefabRoot.AddComponent<FishMovementController>();
                var animator = prefabRoot.GetComponent<FishBoneAnimator>() ?? prefabRoot.AddComponent<FishBoneAnimator>();
                var fishState = prefabRoot.GetComponent<FishState>() ?? prefabRoot.AddComponent<FishState>();
                var body = prefabRoot.GetComponentsInChildren<Transform>(true).First(transform => transform.name == "body");
                var bodyRenderer = body.GetComponent<SpriteRenderer>();
                var spriteSkin = body.GetComponent<SpriteSkin>() ?? body.gameObject.AddComponent<SpriteSkin>();
                var sprite = LoadSprite();

                if (sprite == null)
                {
                    return;
                }

                bodyRenderer.sprite = sprite;
                bodyRenderer.drawMode = SpriteDrawMode.Simple;
                bodyRenderer.size = sprite.rect.size / sprite.pixelsPerUnit;

                var spriteMaterial = prefabRoot
                    .GetComponentsInChildren<SpriteRenderer>(true)
                    .FirstOrDefault(renderer => renderer != bodyRenderer && renderer.sharedMaterial != null)
                    ?.sharedMaterial;

                if (spriteMaterial != null)
                {
                    bodyRenderer.sharedMaterial = spriteMaterial;
                }

                var rigRoot = body.GetComponentsInChildren<Transform>(true).FirstOrDefault(transform => transform.name == "Rig");
                if (rigRoot != null)
                {
                    Object.DestroyImmediate(rigRoot.gameObject);
                }

                rigRoot = new GameObject("Rig").transform;
                rigRoot.SetParent(body, false);

                var bonesByName = new Dictionary<string, Transform>(BoneDefinitions.Length);
                foreach (var definition in BoneDefinitions)
                {
                    var parent = definition.ParentId == -1 ? rigRoot : bonesByName[BoneDefinitions[definition.ParentId].Name];
                    bonesByName[definition.Name] = CreateBoneTransform(parent, definition.Name, definition.Position);
                }

                var orderedBones = BoneDefinitions.Select(definition => bonesByName[definition.Name]).ToArray();

                spriteSkin.alwaysUpdate = true;
                spriteSkin.autoRebind = false;
                spriteSkin.SetRootBone(orderedBones[0]);
                spriteSkin.SetBoneTransforms(orderedBones);
                spriteSkin.ResetBindPose();
                SetBounds(spriteSkin, sprite, 0.58f, 0.56f);

                ConfigureAnimator(animator, movement, body, orderedBones);
                ConfigureMovement(movement);
                ConfigureFishState(fishState, prefabRoot.transform, bonesByName["spine_03"]);
                ConfigureColliders(prefabRoot);
                ConfigureRigidBody(prefabRoot);

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ConfigureAnimator(FishBoneAnimator animator, FishMovementController movement, Transform rootVisual, IReadOnlyList<Transform> orderedBones)
        {
            var serializedObject = new SerializedObject(animator);

            serializedObject.FindProperty("movement").objectReferenceValue = movement;
            serializedObject.FindProperty("rootVisual").objectReferenceValue = rootVisual;
            SetTransformArray(serializedObject.FindProperty("spineBones"), new[]
            {
                orderedBones[HeadBone],
                orderedBones[Spine01Bone],
                orderedBones[Spine02Bone],
                orderedBones[Spine03Bone],
                orderedBones[Spine04Bone],
                orderedBones[Tail01Bone],
                orderedBones[Tail02Bone]
            });
            SetTransformArray(serializedObject.FindProperty("leftFinBones"), new[] { orderedBones[LeftFin01Bone], orderedBones[LeftFin02Bone] });
            SetTransformArray(serializedObject.FindProperty("rightFinBones"), new[] { orderedBones[RightFin01Bone], orderedBones[RightFin02Bone] });
            serializedObject.FindProperty("idleFrequency").floatValue = 0.72f;
            serializedObject.FindProperty("swimFrequency").floatValue = 2.05f;
            serializedObject.FindProperty("idleAmplitude").floatValue = 0.75f;
            serializedObject.FindProperty("swimAmplitude").floatValue = 6.4f;
            serializedObject.FindProperty("secondaryWaveFrequency").floatValue = 1.04f;
            serializedObject.FindProperty("secondaryWaveFactor").floatValue = 0.05f;
            serializedObject.FindProperty("phaseOffset").floatValue = 0.82f;
            serializedObject.FindProperty("turnAngle").floatValue = 5.4f;
            serializedObject.FindProperty("rootSwayOffset").floatValue = 0.012f;
            serializedObject.FindProperty("rootSwayAngle").floatValue = 0.8f;
            serializedObject.FindProperty("rootTurnAngle").floatValue = 1.8f;
            serializedObject.FindProperty("finFrequency").floatValue = 2.1f;
            serializedObject.FindProperty("finAngle").floatValue = 5.6f;
            serializedObject.FindProperty("finTurnAngle").floatValue = 5.2f;
            serializedObject.FindProperty("finPhaseOffset").floatValue = 0.26f;
            serializedObject.FindProperty("animationResponse").floatValue = 4.6f;
            serializedObject.FindProperty("spineAmplitudeCurve").animationCurveValue = new AnimationCurve(
                new Keyframe(0f, 0.005f),
                new Keyframe(0.24f, 0.025f),
                new Keyframe(0.62f, 0.2f),
                new Keyframe(1f, 0.68f));
            serializedObject.FindProperty("spineTurnCurve").animationCurveValue = new AnimationCurve(
                new Keyframe(0f, 0.01f),
                new Keyframe(0.32f, 0.05f),
                new Keyframe(0.7f, 0.22f),
                new Keyframe(1f, 0.52f));

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureMovement(FishMovementController movement)
        {
            var serializedObject = new SerializedObject(movement);
            serializedObject.FindProperty("cruiseSpeed").floatValue = 1.35f;
            serializedObject.FindProperty("escapeSpeed").floatValue = 4.1f;
            serializedObject.FindProperty("turnSmoothTime").floatValue = 0.24f;
            serializedObject.FindProperty("maxTurnSpeed").floatValue = 420f;
            serializedObject.FindProperty("waypointReachDistance").floatValue = 0.5f;
            serializedObject.FindProperty("reactionRadius").floatValue = 3.2f;
            serializedObject.FindProperty("escapeReleaseRadius").floatValue = 4.4f;
            serializedObject.FindProperty("escapeMinDuration").floatValue = 1.05f;
            serializedObject.FindProperty("targetRefreshInterval").floatValue = 3f;
            serializedObject.FindProperty("randomRetargetJitter").floatValue = 0.75f;
            serializedObject.FindProperty("approachOffsetRange").vector2Value = new Vector2(0.6f, 1.8f);
            serializedObject.FindProperty("passOffsetRange").vector2Value = new Vector2(2.2f, 5f);
            serializedObject.FindProperty("passForwardRange").vector2Value = new Vector2(-2.4f, 2.4f);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureFishState(FishState fishState, Transform rootTransform, Transform hookTransform)
        {
            var serializedObject = new SerializedObject(fishState);
            serializedObject.FindProperty("rootTransform").objectReferenceValue = rootTransform;
            serializedObject.FindProperty("hookTransform").objectReferenceValue = hookTransform;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureColliders(GameObject prefabRoot)
        {
            var colliders = prefabRoot.GetComponents<CapsuleCollider2D>().ToList();
            while (colliders.Count < 2)
            {
                colliders.Add(prefabRoot.AddComponent<CapsuleCollider2D>());
            }

            ConfigureCollider(colliders[0], new Vector2(0f, -0.15f), new Vector2(1.08f, 5.45f), CapsuleDirection2D.Vertical);
            ConfigureCollider(colliders[1], new Vector2(0f, 2.82f), new Vector2(2.9f, 0.72f), CapsuleDirection2D.Horizontal);
        }

        private static void ConfigureCollider(CapsuleCollider2D collider2D, Vector2 offset, Vector2 size, CapsuleDirection2D direction)
        {
            collider2D.isTrigger = true;
            collider2D.offset = offset;
            collider2D.size = size;
            collider2D.direction = direction;
        }

        private static void ConfigureRigidBody(GameObject prefabRoot)
        {
            var body = prefabRoot.GetComponent<Rigidbody2D>() ?? prefabRoot.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private static void SetTransformArray(SerializedProperty property, Transform[] values)
        {
            property.arraySize = values.Length;

            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetBounds(SpriteSkin spriteSkin, Sprite sprite, float paddingX, float paddingY)
        {
            if (sprite == null)
            {
                return;
            }

            var size = sprite.bounds.size;
            var bounds = new Bounds(Vector3.zero, new Vector3(size.x + paddingX, size.y + paddingY, 1f));
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
            var bones = new List<SpriteBone>(BoneDefinitions.Length);

            for (var i = 0; i < BoneDefinitions.Length; i++)
            {
                var definition = BoneDefinitions[i];
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
            const int columns = 9;
            const int rows = 21;

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
            var hammer = Band(t, 0.07f, 0.1f) * 0.34f;
            var shoulder = Band(t, 0.46f, 0.18f) * 0.24f;
            var pelvic = Band(t, 0.64f, 0.08f) * 0.05f;
            var tailFin = Band(t, 0.97f, 0.055f) * 0.2f;
            var body = (0.08f + (1f - t) * 0.025f) * (1f - 0.62f * SmoothStep(0.58f, 0.9f, t));
            var neckPinch = Band(t, 0.24f, 0.08f) * 0.045f;
            return width * Mathf.Max(0.03f, hammer + shoulder + pelvic + tailFin + body - neckPinch);
        }

        private static BoneWeight CreateBoneWeight(Vector2 position, float width, float height, float t)
        {
            var halfWidth = Mathf.Max(0.0001f, width * 0.5f);
            var lateral = Mathf.Clamp01(Mathf.Abs(position.x) / halfWidth);
            var outer = SmoothStep(0.32f, 0.84f, lateral);
            var finOuter = SmoothStep(0.68f, 0.98f, lateral);

            if (t <= 0.08f)
            {
                if (position.x < -width * 0.18f)
                {
                    return CreateBlendBoneWeight(LeftHeadBone, 0.88f, HeadBone, 0.12f);
                }

                if (position.x > width * 0.18f)
                {
                    return CreateBlendBoneWeight(RightHeadBone, 0.88f, HeadBone, 0.12f);
                }

                return CreateSingleBoneWeight(HeadBone);
            }

            var weights = new List<BoneInfluence>(8);
            var spinePosition = Mathf.Clamp01(Mathf.InverseLerp(0.14f, 1f, t)) * 5f;
            var spineOffset = Mathf.Clamp(Mathf.FloorToInt(spinePosition), 0, 5);
            var spineIndex = Spine01Bone + spineOffset;
            var nextSpineIndex = Mathf.Min(spineIndex + 1, Tail02Bone);
            var spineBlend = spinePosition - spineOffset;

            AddWeight(weights, spineIndex, 1f - spineBlend);
            AddWeight(weights, nextSpineIndex, spineBlend);

            var headLock = SmoothStep(0.22f, 0.04f, t);
            var neckLock = Band(t, 0.17f, 0.08f);
            var centerLock = Band(t, 0.38f, 0.22f) * SmoothStep(0.7f, 0f, lateral);
            var shoulderSupport = Band(t, 0.46f, 0.18f) * outer;
            var finShoulder = Band(t, 0.46f, 0.12f) * finOuter;
            var finPelvic = Band(t, 0.63f, 0.08f) * finOuter;
            var tailBias = SmoothStep(0.7f, 0.98f, t);

            AddWeight(weights, HeadBone, headLock * Mathf.Lerp(3.2f, 1.4f, outer));
            AddWeight(weights, Spine01Bone, neckLock * 1.4f);
            AddWeight(weights, Spine02Bone, centerLock * 0.45f);
            AddWeight(weights, Spine03Bone, centerLock * 0.4f);
            AddWeight(weights, Tail01Bone, tailBias * 0.35f);
            AddWeight(weights, Tail02Bone, tailBias * 0.85f);

            if (position.x < 0f)
            {
                AddWeight(weights, LeftHeadBone, Band(t, 0.11f, 0.13f) * outer * 2.5f);
                AddWeight(weights, LeftSupportBone, shoulderSupport * 1.7f);
                AddWeight(weights, LeftFin01Bone, finShoulder * 1.1f);
                AddWeight(weights, LeftFin02Bone, finPelvic * 0.95f);
            }
            else if (position.x > 0f)
            {
                AddWeight(weights, RightHeadBone, Band(t, 0.11f, 0.13f) * outer * 2.5f);
                AddWeight(weights, RightSupportBone, shoulderSupport * 1.7f);
                AddWeight(weights, RightFin01Bone, finShoulder * 1.1f);
                AddWeight(weights, RightFin02Bone, finPelvic * 0.95f);
            }

            return NormalizeBoneWeight(weights);
        }

        private static BoneWeight CreateSingleBoneWeight(int boneIndex)
        {
            return new BoneWeight
            {
                boneIndex0 = boneIndex,
                weight0 = 1f
            };
        }

        private static BoneWeight CreateBlendBoneWeight(int primaryIndex, float primaryWeight, int secondaryIndex, float secondaryWeight)
        {
            var total = primaryWeight + secondaryWeight;
            if (total <= 0f)
            {
                return CreateSingleBoneWeight(primaryIndex);
            }

            return new BoneWeight
            {
                boneIndex0 = primaryIndex,
                weight0 = primaryWeight / total,
                boneIndex1 = secondaryIndex,
                weight1 = secondaryWeight / total
            };
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

        private static float SmoothStep(float from, float to, float value)
        {
            if (Mathf.Approximately(from, to))
            {
                return 0f;
            }

            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));
        }

        private static Sprite LoadSprite()
        {
            return AssetDatabase.LoadAllAssetsAtPath(SpritePath)
                .OfType<Sprite>()
                .FirstOrDefault(sprite => sprite.name == SpriteName);
        }

        private static void CleanupMultipleMeta()
        {
            var metaPath = $"{SpritePath}.meta";
            if (!File.Exists(metaPath))
            {
                return;
            }

            var text = File.ReadAllText(metaPath);
            var newline = text.Contains("\r\n") ? "\r\n" : "\n";

            text = ReplaceFirst(text, @"(?ms)^    bones:.*?^    spriteID:", $"    bones: []{newline}    spriteID:");
            text = ReplaceFirst(text, @"(?m)^    internalID: \d+$", "    internalID: 0");
            text = ReplaceFirst(text, @"(?ms)^    vertices:.*?^    indices:.*?$", $"    vertices: []{newline}    indices: ");
            text = ReplaceFirst(text, @"(?ms)^    edges:.*?^    weights:", $"    edges: []{newline}    weights:");
            text = ReplaceFirst(text, @"(?ms)^    weights:.*?^    secondaryTextures:", $"    weights: []{newline}    secondaryTextures:");

            File.WriteAllText(metaPath, text);
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
        }

        private static string ReplaceFirst(string input, string pattern, string replacement)
        {
            return new Regex(pattern, RegexOptions.Multiline | RegexOptions.Singleline).Replace(input, replacement, 1);
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
