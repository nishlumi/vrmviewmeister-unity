using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
//using VRMShaders;
using UniVRM10;


namespace UserHandleSpace
{
    public class CustomVrmaDataStore
    {
        public GameObject pointerObject;
        public PositionExporter m_position;
        public Dictionary<HumanBodyBones, Transform> m_bones;
        public Dictionary<HumanBodyBones, Transform> m_bonesParent;
        public Dictionary<HumanBodyBones, Transform> m_lookats;
        public Dictionary<HumanBodyBones, Transform> m_lookatsParent;
        public List<ExportFrame> m_frames;
        // 表情関連のフィールド追加
        public Vrm10RuntimeExpression runtimeExpression;
        public OperateLoadedVRM operateLoadedVRM;
        public Dictionary<string, Transform> m_expressions;
        public Dictionary<ExpressionKey, float> m_vrm10Expressions;
        public Dictionary<ExpressionKey, int> m_expressionNodeIndices;
        // 視線制御関連のフィールド
        public Transform m_lookAtTarget;
        public Transform m_lookAtNode;
        public int m_lookAtNodeIndex;

        //===============================================================================================
        // child class
        //===============================================================================================

        public class ExportFrame
        {
            public int frameIndex;
            public float duration;

            public Vector3 position;
            public Dictionary<HumanBodyBones, RotationExporter> rotations;
            public Dictionary<HumanBodyBones, RotationExporter> lookats;
            public Dictionary<string, ExpressionData> expressions; // 追加
            //---lookat
            public LookAtData lookAt;


            public ExportFrame(int index, float timef)
            {
                frameIndex = index;
                duration = timef;
                position = Vector3.zero;
                rotations = new Dictionary<HumanBodyBones, RotationExporter>();
                lookats = new Dictionary<HumanBodyBones, RotationExporter>();
                expressions = new Dictionary<string, ExpressionData>(); // 追加
                lookAt = null;
            }

        }
        public class PositionExporter
        {
            public List<Vector3> Values = new();

            public Transform Node;
            public Transform m_root;

            public PositionExporter(Transform bone, Transform root)
            {
                Node = bone;
                m_root = root;
            }
            /*
            public void Add(int inverted = -1)
            {
                var p = m_root.worldToLocalMatrix.MultiplyPoint(Node.position);
                // reverse-X
                Values.Add(new Vector3(p.x * inverted, p.y, p.z));
            }
            public void Modify(int index, int inverted = -1)
            {
                var p = m_root.worldToLocalMatrix.MultiplyPoint(Node.position);
                Values[index] = new Vector3(p.x * inverted, p.y, p.z);
            }
            */
        }
        public class RotationExporter
        {
            public Quaternion Values = new();
            /*
            public readonly Transform Node;
            public Transform m_parent;

            public RotationExporter(Transform bone, Transform parent)
            {
                Node = bone;
                m_parent = parent;
            }
            
            public void Add(int inverted = -1)
            {
                var q = Quaternion.Inverse(m_parent.rotation) * Node.rotation;
                // reverse-X
                Values = new Quaternion(q.x, q.y * inverted, q.z * inverted, q.w);
            }
            public void Modify(int index, int inverted = -1)
            {
                var q = Quaternion.Inverse(m_parent.rotation) * Node.rotation;
                Values = new Quaternion(q.x, q.y * inverted, q.z * inverted, q.w);
            }
            */
        }
        public class TransformInformation
        {
            PositionExporter m_position;
            Dictionary<HumanBodyBones, Transform> m_bones;
            Dictionary<HumanBodyBones, Transform> m_bonesParent;

        }
        public class ExpressionData
        {
            public string expressionName;
            public float weight;
            public int nodeIndex;

            public ExpressionData(string name, float w, int nodeIdx)
            {
                expressionName = name;
                weight = w;
                nodeIndex = nodeIdx;
            }
        }
        public class LookAtData
        {
            public Vector3 direction;
            public int nodeIndex;
            public Transform lookAtNode;
            public Quaternion rotation;

            public LookAtData(Vector3 dir, int nodeIdx, Transform node, Quaternion rotation)
            {
                direction = dir;
                nodeIndex = nodeIdx;
                lookAtNode = node;
                this.rotation = rotation;
            }
        }

        //===============================================================================================
        // constructor
        //===============================================================================================

        public CustomVrmaDataStore()
        {
            m_bones = new Dictionary<HumanBodyBones, Transform>();
            m_bonesParent = new Dictionary<HumanBodyBones, Transform>();
            m_lookats = new Dictionary<HumanBodyBones, Transform>();
            m_lookatsParent = new Dictionary<HumanBodyBones, Transform>();
            m_frames = new List<ExportFrame>();

        }

        //===============================================================================================
        // bone setter
        //===============================================================================================

        public void SetPositionBoneAndParent(Transform bone, Transform parent)
        {
            m_position = new PositionExporter(bone, parent);
        }
        public void AddRotationBoneAndParent(HumanBodyBones bone, Transform transform, Transform parent)
        {
            m_bones.Add(bone, transform);
            m_bonesParent.Add(bone, parent);
        }
        public void AddRotationLookAtBone(HumanBodyBones bone, Transform transform, Transform parent) 
        { 
            m_lookats.Add(bone, transform);
            m_lookatsParent.Add(bone, parent);
        }

        public Quaternion CalculateRotation(Transform parent, Transform node, Vector3 inverted)
        {
                      
            var q = Quaternion.Inverse(parent.rotation) * node.rotation;
            
            // reverse-X
            //return new Quaternion(q.x, q.y * inverted, q.z * inverted, q.w);
            return new Quaternion(q.x, -q.y, -q.z, q.w);            
        }
        public Vector3 CalculateRotationVector(Transform parent, Transform node, Vector3 inverted)
        {
            var q = Quaternion.Inverse(parent.rotation) * node.rotation;
            // reverse-X
            return new Vector3(q.eulerAngles.x * inverted.x, q.eulerAngles.y * inverted.y, q.eulerAngles.z * inverted.z);
        }
        public Vector3 CalculatePosition(Transform root, Transform node, Vector3 inverted)
        {
            var p = root.worldToLocalMatrix.MultiplyPoint(node.position);
            // reverse-X
            //return new Vector3(p.x * inverted.x * -1, p.y, p.z);
            return new Vector3(-p.x, p.y, p.z);
        }
        //===============================================================================================
        // expression setter 
        //===============================================================================================

        public void SetupExpressions(Vrm10Instance vrmInstance)
        {
            if (vrmInstance == null || vrmInstance.Runtime?.Expression == null)
                return;

            m_expressions = new Dictionary<string, Transform>();
            m_vrm10Expressions = new Dictionary<ExpressionKey, float>();
            m_expressionNodeIndices = new Dictionary<ExpressionKey, int>();

            var expressionProxy = vrmInstance.Runtime.Expression;
            var allExpressions = expressionProxy.ExpressionKeys;

            int nodeIndex = 0; // 表情ノードの開始インデックス

            foreach (var expressionKey in allExpressions)
            {
                var expression = expressionProxy; //.GetExpression(expressionKey);
                if (expression != null)
                {
                    m_vrm10Expressions[expressionKey] = expressionProxy.GetWeight(expressionKey);
                    m_expressionNodeIndices[expressionKey] = nodeIndex++;

                    // 仮想ノードのTransformを作成
                    GameObject virtualNode = new GameObject($"{expressionKey.Name}");
                    m_expressions[expressionKey.Name] = virtualNode.transform;
                }
            }
        }

        public void CaptureExpressionWeights(ExportFrame frame)
        {
            operateLoadedVRM.Vrm10Instance.Runtime.Process();
            foreach (var kv in m_vrm10Expressions)
            {
                string expName = kv.Key.Name;
                var expression = kv.Value;

                // 現在のウェイト値を取得
                //float currentWeight = operateLoadedVRM.getProxyBlendShape(expName); // GetCurrentExpressionWeight(expName, ovrm);
                float currentWeight = operateLoadedVRM.Vrm10Instance.Runtime.Expression.GetWeight(kv.Key);

                // ExpressionDataを作成
                var expData = new ExpressionData(expName, currentWeight, m_expressionNodeIndices[kv.Key]);
                frame.expressions[expName] = expData;
            }
        }


        //===============================================================================================
        // look at setter 
        //===============================================================================================
        public void SetupLookAt(Vrm10Instance vrmInstance)
        {
            if (vrmInstance == null || vrmInstance.Runtime?.LookAt == null)
                return;

            var lookAtComponent = vrmInstance.Runtime.LookAt;

            // 視線制御用の仮想ノードを作成
            GameObject lookAtNodeObject = new GameObject("LookAt");
            m_lookAtNode = lookAtNodeObject.transform;
            m_lookAtNodeIndex = 2000; // 視線制御ノードのインデックス

            // 視線のターゲットを設定（必要に応じて）
            if (vrmInstance.LookAtTarget != null)
            {
                m_lookAtTarget = vrmInstance.LookAtTarget;
            }
        }

        public void CaptureLookAtDirection(ExportFrame frame)
        {
            if (m_lookAtNode == null) return;

            // 現在の視線方向を計算
            Vector3 lookDirection = CalculateLookDirection();

            // 視線方向をノードの回転として設定
            //Quaternion.LookRotation(lookDirection);
            Quaternion eyerot = m_lookats[HumanBodyBones.LeftEye].rotation;
            m_lookAtNode.position = m_lookAtTarget.position;

            // LookAtDataを作成
            frame.lookAt = new LookAtData(lookDirection, m_lookAtNodeIndex, m_lookAtNode, eyerot);
        }

        private Vector3 CalculateLookDirection()
        {
            // 実際の視線方向を計算する処理
            // VRMの目の向きやターゲットから計算
            if (m_lookAtTarget != null)
            {
                Vector3 headPosition = m_bones[HumanBodyBones.Head].position;
                Vector3 targetPosition = m_lookAtTarget.position;
                return (targetPosition - headPosition).normalized;
            }

            // デフォルトは正面方向
            return Vector3.forward;
        }

        //===============================================================================================
        // basic method
        //===============================================================================================

        public int GetFrameCount()
        {
            return m_frames.Count;
        }
        public int GetFrameByIndex(int index)
        {
            int ishit = m_frames.FindIndex(ma =>
            {
                if (ma.frameIndex == index) return true;
                return false;
            });

            return ishit;
        }
        public int GetNearMinFrameIndex(int index)
        {
            int ret = -1;
            for (int i = m_frames.Count-1; 0 <= i; i--)
            {
                if (m_frames[i].frameIndex == index) ret = i - 1;
            }

            return ret;
        }
        public int GetNearMaxFrameIndex(int index)
        {
            int ret = -1;
            for (int i = 0; i < m_frames.Count; i++)
            {
                if (m_frames[i].frameIndex == index) ret = i + 1;
            }
            if (ret >= m_frames.Count) ret = -1;
            return ret;
        }
        public ExportFrame GetFrame(int index)
        {
            int ishit = m_frames.FindIndex(ma =>
            {
                if (ma.frameIndex == index) return true;
                return false;
            });

            if (ishit == -1)
            {
                return null;
            }
            else
            {
                return m_frames[ishit];
            }
        }

        //===============================================================================================
        // frame creator
        //===============================================================================================

        public ExportFrame GenerateFrame(int index, float duration, Vector3 inverted)
        {
            ExportFrame frame = new ExportFrame(index, duration);
            frame.position = CalculatePosition(m_position.m_root, m_position.Node, inverted);
            int prevFrameIndex = GetNearMinFrameIndex(index);
            ExportFrame prevFrame = null;
            if ((0 <= prevFrameIndex) && (prevFrameIndex < m_frames.Count))
            {
                prevFrame = m_frames[prevFrameIndex];
            }

            //---For Humanoid
            foreach (var kv in m_bones)
            {
                Vector3 curqua = kv.Value.eulerAngles;
                Vector3 befqua = prevFrame == null ? Vector3.zero : prevFrame.rotations[kv.Key].Values.eulerAngles;

                RotationExporter rote = new RotationExporter();
                /*if (kv.Key == HumanBodyBones.Neck)
                {
                    
                    float invx = curqua.x > befqua.x ? 1 : -1;
                    float invy = curqua.y > befqua.y ? -1 : 1;
                    float invz = curqua.z > befqua.z ? -1 : 1;

                    rote.Values = Quaternion.Euler(CalculateRotationVector(m_bonesParent[kv.Key], kv.Value, new Vector3(invx, invy, invz)));
                }
                else if (kv.Key == HumanBodyBones.Head)
                {
                    float invx = curqua.x > befqua.x ? -1 : 1;
                    float invy = curqua.y > befqua.y ? -1 : 1;
                    float invz = curqua.z > befqua.z ? 1 : -1;

                    rote.Values = Quaternion.Euler(CalculateRotationVector(m_bonesParent[kv.Key], kv.Value, new Vector3(invx, invy, invz)));
                }
                else if (kv.Key == HumanBodyBones.UpperChest)
                {
                    rote.Values = Quaternion.Euler(CalculateRotationVector(m_bonesParent[kv.Key], kv.Value, inverted));
                }
                else if (
                    (kv.Key == HumanBodyBones.RightShoulder) || (kv.Key == HumanBodyBones.LeftShoulder)
                )
                {
                    rote.Values = Quaternion.Euler(CalculateRotationVector(m_bonesParent[kv.Key], kv.Value, new Vector3(1, 1, 1)));
                }
                else*/
                /*
                if ((kv.Key == HumanBodyBones.LeftEye) || (kv.Key == HumanBodyBones.RightEye))
                { //---VRM10ControlBone:****Eye convert to Root/****_Eye
                    rote.Values = CalculateRotation(m_bonesParent[kv.Key], m_lookats[kv.Key], inverted);
                }

                else*/
                {
                    rote.Values = CalculateRotation(m_bonesParent[kv.Key], kv.Value, inverted);
                }

                frame.rotations.Add(kv.Key, rote);
            }

            //---expression
            CaptureExpressionWeights(frame);

            //---lookat
            CaptureLookAtDirection(frame);

            return frame;
        }
        public void AddFrame(int index, float duration, Vector3 inverted)
        {
            m_frames.Add(GenerateFrame(index, duration, inverted));

        }

        public void ModifyFrame(int index, float duration, Vector3 inverted)
        {
            int ishit = GetFrameByIndex(index);
            if (ishit > -1)
            {
                ExportFrame frame = GenerateFrame(index, duration, inverted);
                m_frames[ishit].duration = duration;
                m_frames[ishit].position = frame.position;
                m_frames[ishit].rotations = frame.rotations;
                m_frames[ishit].expressions = frame.expressions;
                m_frames[ishit].lookAt = frame.lookAt;
                m_frames[ishit].lookats = frame.lookats;
            }
        }
        public void DeleteFrame(int index)
        {
            int ishit = GetFrameByIndex(index);
            if (ishit > -1)
            {
                m_frames.RemoveAt(ishit);
                //---Regenerate frame index
                /*for (int i = 0; i < m_frames.Count; i++)
                {
                    //---if frame index is more than delete index, substract frame index (-1)
                    if (m_frames[i].frameIndex > index)
                    {
                        m_frames[i].frameIndex--;
                    }
                }*/
            }
        }
        public void DeleteRowFrame(int index)
        {
            int ishit = GetFrameByIndex(index);
            if (ishit > -1)
            {
                m_frames.RemoveAt(ishit);
                //---Regenerate frame index
                for (int i = 0; i < m_frames.Count; i++)
                {
                    //---if frame index is more than delete index, substract frame index (-1)
                    if (m_frames[i].frameIndex > index)
                    {
                        m_frames[i].frameIndex--;
                    }
                }
            }
        }
        public void InsertFrame(int index, float duration, Vector3 inverted)
        {
            ExportFrame frame = GenerateFrame(index, duration, inverted);
            //---Regenerate frame index
            for (int i = 0; i < m_frames.Count; i++)
            {
                //---if frame index is >= insert index, add frame index (+1)
                if (m_frames[i].frameIndex >= index)
                {
                    m_frames[i].frameIndex++;
                }
            }
            m_frames.Insert(index, frame);

        }
        public void ClearFrames()
        {
            m_frames.Clear();
        }

    }
    //===============================================================================================
    // child class 2
    //===============================================================================================

    public class CustomVrmaExporter : gltfExporter
    {
        public class ExportFrame
        {
            public int frameIndex;
            public float duration;

            public Vector3 position;
            public Dictionary<HumanBodyBones, RotationExporter> rotations;

            public ExportFrame(int index, float timef)
            {
                frameIndex = index;
                duration = timef;
                position = Vector3.zero;
                rotations = new Dictionary<HumanBodyBones, RotationExporter>();
            }
            
        }
        public class PositionExporter
        {
            public List<Vector3> Values = new();

            public Transform Node;
            public Transform m_root;

            public PositionExporter(Transform bone, Transform root)
            {
                Node = bone;
                m_root = root;
            }
            /*
            public void Add(int inverted = -1)
            {
                var p = m_root.worldToLocalMatrix.MultiplyPoint(Node.position);
                // reverse-X
                Values.Add(new Vector3(p.x * inverted, p.y, p.z));
            }
            public void Modify(int index, int inverted = -1)
            {
                var p = m_root.worldToLocalMatrix.MultiplyPoint(Node.position);
                Values[index] = new Vector3(p.x * inverted, p.y, p.z);
            }
            */
        }
        public class RotationExporter
        {
            public Quaternion Values = new();
            /*
            public readonly Transform Node;
            public Transform m_parent;

            public RotationExporter(Transform bone, Transform parent)
            {
                Node = bone;
                m_parent = parent;
            }
            
            public void Add(int inverted = -1)
            {
                var q = Quaternion.Inverse(m_parent.rotation) * Node.rotation;
                // reverse-X
                Values = new Quaternion(q.x, q.y * inverted, q.z * inverted, q.w);
            }
            public void Modify(int index, int inverted = -1)
            {
                var q = Quaternion.Inverse(m_parent.rotation) * Node.rotation;
                Values = new Quaternion(q.x, q.y * inverted, q.z * inverted, q.w);
            }
            */
        }
        public class TransformInformation
        {
            PositionExporter m_position;
            Dictionary<HumanBodyBones, Transform> m_bones;
            Dictionary<HumanBodyBones, Transform> m_bonesParent;

        }


        PositionExporter m_position;
        Dictionary<HumanBodyBones, Transform> m_bones;
        Dictionary<HumanBodyBones, Transform> m_bonesParent;
        List<ExportFrame> m_frames;

        public CustomVrmaExporter(
                ExportingGltfData data,
                GltfExportSettings settings)
        : base(data, settings)
        {
            settings.InverseAxis = Axes.X;

            m_bones = new Dictionary<HumanBodyBones, Transform>();
            m_bonesParent = new Dictionary<HumanBodyBones, Transform>();
            m_frames = new List<ExportFrame>();
        }

        /*
        public void CopyFrameData(ref CustomVrmaExporter dst)
        {
            dst.m_frames.AddRange(m_frames);
        }
        public new void SetPositionBoneAndParent(Transform bone, Transform parent)
        {
            m_position = new PositionExporter(bone, parent);
        }
        public new void AddRotationBoneAndParent(HumanBodyBones bone, Transform transform, Transform parent)
        {
            m_bones.Add(bone, transform);
            m_bonesParent.Add(bone, parent);
        }
        public Quaternion CalculateRotation(Transform parent, Transform node, Vector3 inverted)
        {
            var q = Quaternion.Inverse(parent.rotation) * node.rotation;
            
            // reverse-X
            //return new Quaternion(q.x, q.y * inverted, q.z * inverted, q.w);
            return new Quaternion(q.x, -q.y, -q.z, q.w);
        }
        public Vector3 CalculateRotationVector(Transform parent, Transform node, Vector3 inverted)
        {
            var q = Quaternion.Inverse(parent.rotation) * node.localRotation;
            // reverse-X
            return new Vector3(q.eulerAngles.x * inverted.x, q.eulerAngles.y * inverted.y, q.eulerAngles.z * inverted.z);
        }
        public Vector3 CalculatePosition(Transform root, Transform node, Vector3 inverted)
        {
            var p = root.worldToLocalMatrix.MultiplyPoint(node.position);
            // reverse-X
            //return new Vector3(p.x * inverted.x * -1, p.y, p.z);
            return new Vector3(-p.x, p.y, p.z);
        }
        */

        /*

        public int GetFrameCount()
        {
            return m_frames.Count;
        }
        public int GetFrameByIndex(int index)
        {
            int ishit = m_frames.FindIndex(ma =>
            {
                if (ma.frameIndex == index) return true;
                return false;
            });

            return ishit;
        }
        public ExportFrame GetFrame(int index)
        {
            int ishit = m_frames.FindIndex(ma =>
            {
                if (ma.frameIndex == index) return true;
                return false;
            });

            if(ishit == -1)
            {
                return null;
            }
            else
            {
                return m_frames[ishit];
            }
        }
                
        public ExportFrame GenerateFrame(int index, float duration, Vector3 inverted)
        {
            ExportFrame frame = new ExportFrame(index, duration);
            frame.position = CalculatePosition(m_position.m_root, m_position.Node, inverted);

            foreach (var kv in m_bones)
            {
                RotationExporter rote = new RotationExporter();
                if (kv.Key == HumanBodyBones.Neck)
                {
                    rote.Values = CalculateRotation(m_bonesParent[kv.Key], kv.Value, new Vector3(inverted.x, inverted.y * 0, inverted.z));
                }
                else
                {
                    rote.Values = CalculateRotation(m_bonesParent[kv.Key], kv.Value, inverted);
                }
                


                frame.rotations.Add(kv.Key, rote);
            }
            return frame;
        }
        public void AddFrame(int index, float duration, Vector3 inverted)
        {
            m_frames.Add(GenerateFrame(index, duration, inverted));

        }

        public void ModifyFrame(int index, float duration, Vector3 inverted)
        {
            int ishit = GetFrameByIndex(index);
            if (ishit > -1)
            {
                ExportFrame frame = GenerateFrame(index, duration, inverted);
                m_frames[ishit].duration = duration;
                m_frames[ishit].position = frame.position;
                m_frames[ishit].rotations = frame.rotations;
            }                       
        }
        public void DeleteFrame(int index)
        {
            int ishit = GetFrameByIndex(index);
            if (ishit > -1)
            {
                m_frames.RemoveAt(ishit);
                //---Regenerate frame index
                for (int i = 0; i < m_frames.Count; i++)
                {
                    //---if frame index is more than delete index, substract frame index (-1)
                    if (m_frames[i].frameIndex > index)
                    {
                        m_frames[i].frameIndex--;
                    }
                }
            }
        }
        public void InsertFrame(int index, float duration, Vector3 inverted)
        {
            ExportFrame frame = GenerateFrame(index, duration, inverted);
            //---Regenerate frame index
            for (int i = 0; i < m_frames.Count; i++)
            {
                //---if frame index is >= insert index, add frame index (+1)
                if (m_frames[i].frameIndex >= index)
                {
                    m_frames[i].frameIndex++;
                }
            }
            m_frames.Insert(index, frame);

        }
        public void ClearFrames()
        {
            m_frames.Clear();
        }
        */

        //===============================================================================================
        // generational methods for frame finalize
        //===============================================================================================


        public List<float> GenerateTotalSeconds(List<CustomVrmaDataStore.ExportFrame> frames)
        {

            List<float> m_totaltimes = new List<float>();

            float total = 0f;
            //m_totaltimes.Add(0);
            for (int i = 0; i < frames.Count; i++)
            {
                if (i == 0)
                {
                    total += 0f;
                    m_totaltimes.Add(0f);
                }
                else
                {
                    total += frames[i].duration;
                    m_totaltimes.Add(total);
                }
                
                
            }
            return m_totaltimes;
        }
        public List<Vector3> GenerateTotalPositions(List<CustomVrmaDataStore.ExportFrame> frames)
        {
            List<Vector3> ret = new List<Vector3>();
            for (int i = 0; i < frames.Count; i++)
            {
                ret.Add(frames[i].position);
            }
            return ret;
        }
        public List<Quaternion> GenerateTotalRotations(HumanBodyBones bone, List<CustomVrmaDataStore.ExportFrame> frames)
        {
            List<Quaternion> ret = new List<Quaternion>();
            for (int i = 0; i < frames.Count; i++)
            {
                ret.Add(frames[i].rotations[bone].Values);
            }
            return ret;
        }

        public List<CustomVrmaDataStore.ExportFrame> SortFrames(List<CustomVrmaDataStore.ExportFrame> frames)
        {
            List<CustomVrmaDataStore.ExportFrame> ret = new List<CustomVrmaDataStore.ExportFrame>();
            ret.AddRange(frames);
            ret.Sort((a, b) =>
            {
                return a.frameIndex - b.frameIndex;
            });

            return ret;
        }
        //============================================================================================
        //  Export expression
        //============================================================================================

        public void ExportExpressionsAnimation(CustomVrmaDataStore datastore,
                                           List<CustomVrmaDataStore.ExportFrame> frames,
                                           glTFAnimation gltfAnimation,
                                           int inputAccessor, int nodeNextCount)
        {
            var names = Nodes.Select(x => x.name).ToList();
            int nodeidx = nodeNextCount;

            // 各表情のアニメーションを出力
            // expression[frame, frame, frame] data struct !
            foreach (var expName in datastore.m_vrm10Expressions.Keys)
            {
                var weightValues = GenerateExpressionWeights(expName.Name, frames);
                var positionValues = weightValues.Select(w => new Vector3(w, 0, 0)).ToArray();

                var output = _data.ExtendBufferAndGetAccessorIndex(positionValues, glBufferTarget.ARRAY_BUFFER);
                var sampler = gltfAnimation.samplers.Count;

                gltfAnimation.samplers.Add(new glTFAnimationSampler
                {
                    input = inputAccessor,
                    output = output,
                    interpolation = "LINEAR",
                });

                // 表情用の仮想ノードを追加
                int nodeIndex = AddVirtualExpressionNode(expName.Name, datastore);

                gltfAnimation.channels.Add(new glTFAnimationChannel
                {
                    sampler = sampler,
                    target = new glTFAnimationTarget
                    {
                        node = nodeIndex,
                        path = "translation",
                    },
                });
                datastore.m_expressionNodeIndices[expName] = nodeIndex;
                nodeidx++;
            }
        }

        private List<float> GenerateExpressionWeights(string expressionName,
                                                    List<CustomVrmaDataStore.ExportFrame> frames)
        {
            List<float> weights = new List<float>();

            foreach (var frame in frames)
            {
                if (frame.expressions.ContainsKey(expressionName))
                {
                    weights.Add(frame.expressions[expressionName].weight);
                }
                else
                {
                    weights.Add(0f);
                }
            }

            return weights;
        }

        private int AddVirtualExpressionNode(string expressionName, CustomVrmaDataStore datastore)
        {
            // 仮想ノードをglTFノードリストに追加
            var virtualNode = new glTFNode
            {
                name = $"{expressionName}",
                //translation = new float[] { 1, 0, 0 },
                //rotation = new float[] { 0, 0, 0, 0 },
                //scale = new float[] { 0, 0, 0 }
            };

            _data.Gltf.nodes.Add(virtualNode);
            Nodes.Add(datastore.m_expressions[expressionName]);

            return _data.Gltf.nodes.Count - 1;
        }

        //============================================================================================
        //  Export lookat
        //============================================================================================
        public int ExportLookAtAnimation(CustomVrmaDataStore datastore,
                                     List<CustomVrmaDataStore.ExportFrame> frames,
                                     glTFAnimation gltfAnimation,
                                     int inputAccessor)
        {
            if (datastore.m_lookAtNode == null) return -1;

            var lookAtRotations = GenerateLookAtRotations(frames);
            var lookAtTranslations = GenerateLookAtTranslation(frames);
            var output = _data.ExtendBufferAndGetAccessorIndex(lookAtRotations.ToArray());
            var output2 = _data.ExtendBufferAndGetAccessorIndex(lookAtTranslations.ToArray());
            var sampler = gltfAnimation.samplers.Count;

            gltfAnimation.samplers.Add(new glTFAnimationSampler
            {
                input = inputAccessor,
                output = output,
                //output = output2,
                interpolation = "LINEAR",
            });

            // 視線制御ノードを追加
            int nodeIndex = AddLookAtNode(datastore);

            gltfAnimation.channels.Add(new glTFAnimationChannel
            {
                sampler = sampler,
                target = new glTFAnimationTarget
                {
                    node = nodeIndex,
                    path = "rotation",
                    //path = "translation",
                },
            });
            return nodeIndex;
        }

        private List<Quaternion> GenerateLookAtRotations(List<CustomVrmaDataStore.ExportFrame> frames)
        {
            List<Quaternion> rotations = new List<Quaternion>();

            foreach (var frame in frames)
            {
                if (frame.lookAt != null)
                {
                    rotations.Add(frame.lookAt.rotation);
                }
                else
                {
                    rotations.Add(Quaternion.identity);
                }
            }

            return rotations;
        }
        private List<Vector3> GenerateLookAtTranslation(List<CustomVrmaDataStore.ExportFrame> frames)
        {
            List<Vector3> translations = new List<Vector3>();

            foreach (var frame in frames)
            {
                if (frame.lookAt != null)
                {
                    translations.Add(frame.lookAt.lookAtNode.position);
                }
                else
                {
                    translations.Add(Vector3.zero);
                }
            }

            return translations;
        }

        private int AddLookAtNode(CustomVrmaDataStore datastore)
        {
            var lookAtNode = new glTFNode
            {
                name = "LookAt",
                translation = new float[] { 0, 0, 0 },
                rotation = new float[] { 0, 0, 0, 1 },
                scale = new float[] { 1, 1, 1 }
            };

            _data.Gltf.nodes.Add(lookAtNode);
            Nodes.Add(datastore.m_lookAtTarget);

            return _data.Gltf.nodes.Count - 1;
        }



        //===============================================================================================
        // export finally
        //===============================================================================================

        public void ExportCustom(CustomVrmaDataStore datastore, List<CustomVrmaDataStore.ExportFrame> targetFrames, string title = "")
        {

            //_data.Gltf.extensions = new glTFExtensionExport();

            //Export(new RuntimeTextureSerializer());
            //base.Export(new RuntimeTextureSerializer());
            //---Nodes is recreate here !
            base.Export();

            //---copy and sort frames
            List<CustomVrmaDataStore.ExportFrame> frames = SortFrames(targetFrames);

            //
            // export (custom version)
            // 
            var gltfAnimation = new glTFAnimation
            {
            };
            gltfAnimation.name = title;
            _data.Gltf.animations.Clear();
            _data.Gltf.animations.Add(gltfAnimation);

            // this.Nodes には 右手左手変換後のコピーが入っている
            // 代替として名前で逆引きする
            var names = Nodes.Select(x => x.name).ToList();
            for (int i = 0; i < names.Count; i++)
            {
                Debug.Log(i.ToString() + "=" + names[i]);
            }

            // time values
            var input = _data.ExtendBufferAndGetAccessorIndex(GenerateTotalSeconds(frames).ToArray());

            {
                var output = _data.ExtendBufferAndGetAccessorIndex(GenerateTotalPositions(frames).ToArray());
                var sampler = gltfAnimation.samplers.Count;
                gltfAnimation.samplers.Add(new glTFAnimationSampler
                {
                    input = input,
                    output = output,
                    interpolation = "LINEAR",
                });

                gltfAnimation.channels.Add(new glTFAnimationChannel
                {
                    sampler = sampler,
                    target = new glTFAnimationTarget
                    {
                        node = names.IndexOf(datastore.m_position.Node.name),
                        path = "translation",
                    },
                });
            }

            foreach (var kv in datastore.m_bones)
            {
                var output = _data.ExtendBufferAndGetAccessorIndex(GenerateTotalRotations(kv.Key, frames).ToArray());
                var sampler = gltfAnimation.samplers.Count;
                gltfAnimation.samplers.Add(new glTFAnimationSampler
                {
                    input = input,
                    output = output,
                    interpolation = "LINEAR",
                });

                gltfAnimation.channels.Add(new glTFAnimationChannel
                {
                    sampler = sampler,
                    target = new glTFAnimationTarget
                    {
                        node = names.IndexOf(kv.Value.name),
                        path = "rotation",
                    },
                });
                Debug.Log(kv.Key.ToString() + "_" + names.IndexOf(kv.Value.name).ToString() + "_" + kv.Value.name);
            }

            // VRMC_vrm_animation
            var vrmAnimation = VrmAnimationUtil.Create(datastore.m_bones, names);
            vrmAnimation.Extensions = new glTFExtensionExport();
            vrmAnimation.Extras = new glTFExtensionExport();


            //---TEST:expression
            ExportExpressionsAnimation(datastore, frames, gltfAnimation, input, names.Count);

            //---TEST:lookat
            int finalLookAtNodeIndex = ExportLookAtAnimation(datastore, frames, gltfAnimation, input);


            //---expression: set and finallize
            int expNodeIdx = names.Count;
            vrmAnimation.Expressions = new UniGLTF.Extensions.VRMC_vrm_animation.Expressions
            {
                Preset = new UniGLTF.Extensions.VRMC_vrm_animation.Preset(),
                Custom = new Dictionary<string, UniGLTF.Extensions.VRMC_vrm_animation.Expression>(),
                //Extensions = new glTFExtensionExport(),
                //Extras = new glTFExtensionExport()
            };

            foreach (var expNode in datastore.m_expressionNodeIndices)
            {
                var expression = new UniGLTF.Extensions.VRMC_vrm_animation.Expression
                {
                    Node = expNode.Value,
                    //Extensions = new glTFExtensionExport(),
                    //Extras = new glTFExtensionExport()
                };

                // プリセット表情かカスタム表情かを判定
                switch (expNode.Key.Name.ToLower())
                {
                    case "aa": vrmAnimation.Expressions.Preset.Aa = expression; break;
                    case "ih": vrmAnimation.Expressions.Preset.Ih = expression; break;
                    case "ou": vrmAnimation.Expressions.Preset.Ou = expression; break;
                    case "ee": vrmAnimation.Expressions.Preset.Ee = expression; break;
                    case "oh": vrmAnimation.Expressions.Preset.Oh = expression; break;
                    case "happy": vrmAnimation.Expressions.Preset.Happy = expression; break;
                    case "relaxed": vrmAnimation.Expressions.Preset.Relaxed = expression; break;
                    case "angry": vrmAnimation.Expressions.Preset.Angry = expression; break;
                    case "sad": vrmAnimation.Expressions.Preset.Sad = expression; break;
                    case "surprised": vrmAnimation.Expressions.Preset.Surprised = expression; break;
                    case "blink": vrmAnimation.Expressions.Preset.Blink = expression; break;
                    case "blinkleft": vrmAnimation.Expressions.Preset.BlinkLeft = expression; break;
                    case "blinkright": vrmAnimation.Expressions.Preset.BlinkRight = expression; break;
                    case "neutral": vrmAnimation.Expressions.Preset.Neutral = expression; break;
                    default: vrmAnimation.Expressions.Custom[expNode.Key.Name] = expression; break;
                }
                expNodeIdx++;
            }

            //---TEST: LookAt bone new code
            datastore.m_lookAtNodeIndex = finalLookAtNodeIndex;
            vrmAnimation.LookAt = new UniGLTF.Extensions.VRMC_vrm_animation.LookAt
            {
                Node = finalLookAtNodeIndex,

            };       
            



            //Debug.Log(vrmAnimation.LookAt.Node.ToString()+"\r\n"+ datastore.m_bones[HumanBodyBones.LeftEye].name);
            
            UniGLTF.Extensions.VRMC_vrm_animation.GltfSerializer.SerializeTo(
                ref _data.Gltf.extensions,
                vrmAnimation
            );
            
        }
    }
}