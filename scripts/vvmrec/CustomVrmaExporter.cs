using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using VRMShaders;
using UniVRM10;

namespace UserHandleSpace
{
    public class CustomVrmaDataStore
    {
        public class ExportFrame
        {
            public int frameIndex;
            public float duration;

            public Vector3 position;
            public Dictionary<HumanBodyBones, RotationExporter> rotations;
            public Dictionary<HumanBodyBones, RotationExporter> lookats;

            public ExportFrame(int index, float timef)
            {
                frameIndex = index;
                duration = timef;
                position = Vector3.zero;
                rotations = new Dictionary<HumanBodyBones, RotationExporter>();
                lookats = new Dictionary<HumanBodyBones, RotationExporter>();
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
        public GameObject pointerObject;
        public PositionExporter m_position;
        public Dictionary<HumanBodyBones, Transform> m_bones;
        public Dictionary<HumanBodyBones, Transform> m_bonesParent;
        public Dictionary<HumanBodyBones, Transform> m_lookats;
        public Dictionary<HumanBodyBones, Transform> m_lookatsParent;
        public List<ExportFrame> m_frames;

        public CustomVrmaDataStore()
        {
            m_bones = new Dictionary<HumanBodyBones, Transform>();
            m_bonesParent = new Dictionary<HumanBodyBones, Transform>();
            m_lookats = new Dictionary<HumanBodyBones, Transform>();
            m_lookatsParent = new Dictionary<HumanBodyBones, Transform>();
            m_frames = new List<ExportFrame>();

        }
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
                if ((kv.Key == HumanBodyBones.LeftEye) || (kv.Key == HumanBodyBones.RightEye))
                { //---VRM10ControlBone:****Eye convert to Root/****_Eye
                    rote.Values = CalculateRotation(m_bonesParent[kv.Key], m_lookats[kv.Key], inverted);
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

        public List<float> GenerateTotalSeconds(List<CustomVrmaDataStore.ExportFrame> frames)
        {

            List<float> m_totaltimes = new List<float>();

            float total = 0f;
            //m_totaltimes.Add(0);
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i].duration;
                m_totaltimes.Add(total);
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
        public void ExportCustom(CustomVrmaDataStore datastore, List<CustomVrmaDataStore.ExportFrame> targetFrames, string title = "")
        {

            //_data.Gltf.extensions = new glTFExtensionExport();

            //Export(new RuntimeTextureSerializer());
            base.Export(new RuntimeTextureSerializer());

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

            //---TEST: LookAt bone (LeftEye, RightEye) no lookat ???
            /*foreach (var kv in datastore.m_lookats)
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
            }*/

            

            // VRMC_vrm_animation
            var vrmAnimation = VrmAnimationUtil.Create(datastore.m_bones, names);

            vrmAnimation.LookAt = new UniGLTF.Extensions.VRMC_vrm_animation.LookAt
            {
                Node = names.IndexOf(datastore.m_bones[HumanBodyBones.LeftEye].name),
                Extensions = new glTFExtensionExport(),
                Extras = new glTFExtensionExport()
                
            };



            Debug.Log(vrmAnimation.LookAt.Node.ToString()+"\r\n"+ datastore.m_bones[HumanBodyBones.LeftEye].name);
            
            UniGLTF.Extensions.VRMC_vrm_animation.GltfSerializer.SerializeTo(
                ref _data.Gltf.extensions,
                vrmAnimation
            );
            
        }
    }
}