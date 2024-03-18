using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using gc_components;

using CardGame.Data;
using CardGame.Mono;
using CardGame.Animation;

namespace CardGame.Managers
{
    public class MemoryManager
    {
        Queue<StackTransferAnimation> stack_transfer_animation_cache = new Queue<StackTransferAnimation>();
        Queue<TransferAnimation> transfer_animation_cache = new Queue<TransferAnimation>();
        Queue<Interpolator> interpolator_cache = new Queue<Interpolator>();

        public Link[,] container_links = new Link[2, 9];

        public MemoryManager()
        {}

        public void destroy()
        {}

        // ----------------------------------------------------------------------------------
        // -- Animations.
        // ----------------------------------------------------------------------------------

        public StackTransferAnimation aquire_stack_transfer_animation()
        {
            if (stack_transfer_animation_cache.Count == 0)
                stack_transfer_animation_cache.Enqueue(new StackTransferAnimation());

            StackTransferAnimation animation = stack_transfer_animation_cache.Dequeue();

            return animation;
        }

        public void release_stack_transfer_animation(StackTransferAnimation a)
        {
            if (a == null)
                return;

            // Debug.Log("[MemoryManager]: StackTransferAnimation released.");
            stack_transfer_animation_cache.Enqueue(a);
        }

        public TransferAnimation aquire_transfer_animation()
        {
            if (transfer_animation_cache.Count == 0)
                transfer_animation_cache.Enqueue(new TransferAnimation());

            TransferAnimation animation = transfer_animation_cache.Dequeue();
            animation.reset();

            return animation;
        }

        public void release_transfer_animation(TransferAnimation a)
        {
            if (a == null)
                return;

            // Debug.Log("[MemoryManager]: TransferAnimation released.");
            transfer_animation_cache.Enqueue(a);
        }
    }
}
