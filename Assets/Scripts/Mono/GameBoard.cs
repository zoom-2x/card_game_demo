using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Mono
{
    public class GameBoard : MonoBehaviour
    {
        private bool animation_started = false;
        private float animation_time = 1.5f;
        private float animation_t = 0.0f;
        private float animation_current_time = 0.0f;

        GameObject card = null;

        Vector3 start_position;
        Vector3 end_position;
        Vector3 start_rotation;
        Vector3 end_rotation;

        Quaternion start;
        Quaternion end;

        // Start is called before the first frame update
        void Start()
        {
            card = GameObject.Find("CardSquare");

            GameObject deck_1 = GameObject.Find("Deck1");
            GameObject deck_2 = GameObject.Find("Deck2");

            // Vector3 forward_check = new Vector3(card.transform.forward.x, 0, card.transform.forward.z);
            // Vector3 right_check = new Vector3(card.transform.right.)

            card.transform.parent = deck_2.transform;
            animation_started = true;

            start = deck_1.transform.rotation;
            end = deck_2.transform.rotation;

            start_position = card.transform.localPosition;
            start_rotation = card.transform.localEulerAngles;

            if (start_rotation.x > 180)
                start_rotation.x -= 360;

            if (start_rotation.y > 180)
                start_rotation.y -= 360;

            if (start_rotation.z > 180)
                start_rotation.z -= 360;

            end_rotation = new Vector3(0, 0, 0);
            end_position = new Vector3(0, 0, 0);

            print(start_rotation);
        }

        // Update is called once per frame
        void Update()
        {
            if (animation_started)
            {
                Vector3 position = new Vector3();

                animation_current_time += Time.deltaTime;
                animation_t = animation_current_time / animation_time;

                animation_t = 1 - animation_t;
                animation_t = 1.0f - animation_t * animation_t * animation_t;

                if (animation_t < 1.0f)
                {
                    position = start_position + (end_position - start_position) * animation_t;

                    card.transform.localPosition = position;
                    card.transform.rotation = Quaternion.Lerp(start, end, animation_t);
                }
                else
                {
                    animation_started = false;
                    animation_t = 0.0f;
                    animation_current_time = 0.0f;
                }
            }
        }
    }
}
