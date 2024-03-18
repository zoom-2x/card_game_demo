using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using CardGame.Mono;
using CardGame.Managers;
using CardGame.Data;

namespace CardGame.Animation
{

    // Class used to represent a link between two card containers.
    public class Link : MonoBehaviour 
    {
        public const uint REVERSED = 1;
        public const uint FLIP = 2;

        // Bezier control tangent offset vector (world space).
        public Vector3 bezier_control_vector_0 = Vector3.up;
        public Vector3 bezier_control_vector_1 = Vector3.up;

        // Link endpoints.
        public CardContainerMono container_0;
        public CardContainerMono container_1;

        // Animation duration for a single unit (seconds)
        public float duration_sec = 1.0f;
        public float offset = 0;
        [Range(0, 1)]
        public float reposition_threshold = 0;
        public float float_magnitude = 1.0f;

        void OnEnable()
        {
            if (duration_sec == 0)
                duration_sec = 1.0f;
        }

        public TransferAnimation get_transfer_animation(CardMono card, uint setup_mask)
        {
            if (container_0 == null || container_1 == null || card == null)
                return null;

            TransferAnimation unit = GameSystems.memory_manager.aquire_transfer_animation();

            CardContainerMono src = container_0;
            CardContainerMono dest = container_1;

            Vector3 control_vector_0 = bezier_control_vector_0;
            Vector3 control_vector_1 = bezier_control_vector_1;

            if ((setup_mask & REVERSED) > 0)
            {
                src = container_1;
                dest = container_0;

                control_vector_0 = bezier_control_vector_1;
                control_vector_1 = bezier_control_vector_0;
            }

            bool right_to_left = Vector3.Dot(src.transform.position - dest.transform.position, dest.transform.right) >= 0;
            float rotation_sign = right_to_left ? 1 : -1;
            Vector3 rotation_vector = new Vector3(0, 0, 180);

            src.remove_card(card.index);
            src.reposition_reset();
            dest.push_card(card);
            // card.base_position_t = dest.card_orientation_position_t(card);
            // Debug.Log($"base_position_t: {card.index} / {card.base_position_t}");
            // Debug.Log(dest.orientation.relative_position(0));
            
            // Get the dest local position and rotation necessary for the transfer animation.
            // Vector3 local_start_position = dest.transform.InverseTransformPoint(card.transform.position);
            // Quaternion local_start_rotation = Quaternion.Inverse(dest.transform.rotation) * card.transform.rotation;
            // Vector3 local_scale = card.transform.localScale;

            unit.start_position = card.transform.localPosition;
            unit.start_scale = card.transform.localScale;
            unit.start_rotation = Utils.extract_euler_direction(card.transform.localRotation.eulerAngles);
            unit.end_rotation = dest.card_orientation_rotation(card);

            // int dest_index = dest.orientation.virtual_count++;

            // Work is done in the destination container's local coordinates.
            // card.index = dest_index;
            // card.transform.parent = dest.transform;
            card.flags |= CardMono.IN_TRANSIT;
            card.unset_flag(CardMono.FLIPPED);

            unit.card = card;
            unit.src = src;
            unit.dest = dest;

            unit.threshold_reached = false;
            unit.threshold = reposition_threshold;
            unit.t = 0;
            unit.time = 0;
            
            unit.set_duration(duration_sec);
            
            float angle_z_abs = Mathf.Abs(unit.start_rotation.z);

            if (180 - angle_z_abs < angle_z_abs - 0)
            {
                if (unit.start_rotation.z < 0)
                    unit.end_rotation.z = -180;
                else
                    unit.end_rotation.z = 180;
            }
            else
                unit.end_rotation.z = 0;

            if ((setup_mask & FLIP) > 0)
            {
                float flip_angle = 0;

                if (right_to_left)
                {
                    if (unit.start_rotation.z > 0)
                    {
                        if (angle_z_abs < 90)
                            flip_angle = 180;
                        else
                            flip_angle = 360;
                    }
                    else
                    {
                        if (angle_z_abs < 90)
                            flip_angle = 180;
                        else
                            flip_angle = 0;
                    }
                }
                else
                {
                    if (unit.start_rotation.z > 0)
                    {
                        if (angle_z_abs < 90)
                            flip_angle = -180;
                        else
                            flip_angle = 0;
                    }
                    else
                    {
                        if (angle_z_abs < 90)
                            flip_angle = -180;
                        else
                            flip_angle = -360;
                    }
                }

                unit.end_rotation.z = flip_angle;

                if (flip_angle > 0)
                    card.set_flag(CardMono.FLIPPED);
            }

            
            // Vector3 start_position = card.transform.localPosition;
            Vector3 end_position = dest.card_orientation_position(card);

            float bc0_mag = control_vector_0.magnitude;
            float bc1_mag = control_vector_1.magnitude;

            Vector3 bc0 = dest.transform.InverseTransformVector(control_vector_0).normalized * bc0_mag;
            Vector3 bc1 = dest.transform.InverseTransformVector(control_vector_1).normalized * bc1_mag;

            unit.curve.setup(unit.start_position, end_position, unit.start_position + bc0, end_position + bc1);
            // Debug.Log($"positions: {unit.start_position} / {end_position}");

            unit.start_control_vector = bc0;
            unit.end_control_vector = bc1;

            return unit;
        }

        // Floating single card transfer.
        public TransferAnimation transfer_float(CardMono card, uint setup_mask = 0)
        {
            if (card == null)
                return null;

            CardContainerMono src = container_0;
            CardContainerMono dest = container_1;

            if ((setup_mask & REVERSED) > 0)
            {
                src = container_1;
                dest = container_0;
            }

            TransferAnimation unit = GameSystems.memory_manager.aquire_transfer_animation();

            src.remove_card(card.index);
            src.reposition_reset();
            dest.push_card(card);

            card.flags |= CardMono.IN_TRANSIT;
            
            // Get the dest local position and rotation necessary for the transfer animation.
            Vector3 world_position = card.transform.position;

            unit.start_position = card.transform.localPosition;
            unit.start_scale = card.transform.localScale;
            unit.start_rotation = Utils.extract_euler_direction(card.transform.localRotation.eulerAngles);
            unit.end_rotation = dest.card_orientation_rotation(card);

            // world-space.
            Vector3 view_position = GameSystems.game.scene_camera.WorldToViewportPoint(world_position);
            Vector3 slide_vector = GameSystems.input.mouse_slide_vector(view_position.z);

            Vector3 bc0 = float_magnitude * dest.transform.InverseTransformVector(slide_vector);
            Vector3 bc1 = float_magnitude * Vector3.up; 

            unit.card = card;
            unit.src = src;
            unit.dest = dest;

            unit.threshold_reached = false;

            unit.t = 0;
            unit.time = 0;
            unit.set_duration(duration_sec);

            Vector3 end_position = dest.card_orientation_position(card);
            unit.start_control_vector = bc0;
            unit.end_control_vector = bc1;

            unit.curve.setup(unit.start_position, end_position, unit.start_position + bc0, end_position + bc1);
            // unit.curve.setup(start_position, end_position, start_position + bc_0, end_position + bc_1);

            #if UNITY_EDITOR
            dest.debug_curve = unit.curve;
            #endif

            return unit;
        }

        // ----------------------------------------------------------------------------------

        #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (container_0 == null || container_1 == null)
                return;

            Vector3 end_point_0 = container_0.transform.position + bezier_control_vector_0;
            Vector3 end_point_1 = container_1.transform.position + bezier_control_vector_1;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(container_0.transform.position, end_point_0);
            Gizmos.DrawLine(container_1.transform.position, end_point_1);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(end_point_0, 0.05f);
            Gizmos.DrawSphere(end_point_1, 0.05f);

            Handles.DrawBezier(container_0.transform.position, container_1.transform.position,
                               end_point_0, end_point_1, Color.white, null, 2.0f);
        }
        #endif
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(Link))]
    public class LinkEditor : Editor
    {
        SerializedProperty bezier_control_vector_0;
        SerializedProperty bezier_control_vector_1;

        void OnEnable()
        {
            bezier_control_vector_0 = serializedObject.FindProperty("bezier_control_vector_0");
            bezier_control_vector_1 = serializedObject.FindProperty("bezier_control_vector_1");
        }

        void OnSceneGUI()
        {
            Link link = target as Link;

            if (link.container_0 == null || link.container_1 == null)
                return;

            Vector3 end_point_0 = link.container_0.transform.position + bezier_control_vector_0.vector3Value;
            Vector3 end_point_1 = link.container_1.transform.position + bezier_control_vector_1.vector3Value;

            Vector3 new_end_point_0 = Handles.PositionHandle(end_point_0, Quaternion.identity);
            Vector3 new_end_point_1 = Handles.PositionHandle(end_point_1, Quaternion.identity);

            serializedObject.Update();

            bezier_control_vector_0.vector3Value = new_end_point_0 - link.container_0.transform.position;
            bezier_control_vector_1.vector3Value = new_end_point_1 - link.container_1.transform.position;

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
    }
