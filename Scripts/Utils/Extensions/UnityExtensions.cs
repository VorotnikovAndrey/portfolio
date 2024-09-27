using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source
{
    public static class UnityExtensions
    {
        #region ParticleSystem
        public static void SetEmissionRateOverTime(this ParticleSystem ps, float rate)
        {
            var em = ps.emission;
            var r = em.rateOverTime;
            r.mode = ParticleSystemCurveMode.Constant;
            r.constantMin = rate;
            r.constantMax = rate;
            em.rateOverTime = r;
        }

        public static void SetEmissionRateOverDist(this ParticleSystem ps, float rate)
        {
            var em = ps.emission;
            var r = em.rateOverDistance;
            r.mode = ParticleSystemCurveMode.Constant;
            r.constantMin = rate;
            r.constantMax = rate;
            em.rateOverDistance = r;
        }

        public static void SetCurrentParticlesVelocity(this ParticleSystem ps, Vector3 velocity)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
            int count = ps.GetParticles(particles);

            for (int i = 0; i < count; i++)
                particles[i].velocity = velocity;

            ps.SetParticles(particles, count);
        }

        public static void DestroyAllParticles(this ParticleSystem ps)
        {
            ps.SetParticles(null, 0);
        }
        
        public static void SetColor(this ParticleSystem ps, Color c)
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(c);
        }

        public static void SetPlaying(this ParticleSystem ps, bool playing, bool withChildren = true)
        {
            if (playing)
                ps.Play(withChildren);
            else
                ps.Stop(withChildren);
        }
        #endregion

        #region Other

        public static Component CopyComponent(this GameObject destination, Component original)
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }

        public static void SetAlpha(this SpriteRenderer renderer, float alpha)
        {
            renderer.color = new Color(renderer.color.r,
                renderer.color.g,
                renderer.color.b,
                alpha);
        }

        public static void SetActiveSafe(this GameObject go, bool active)
        {
            if (go == null)
                return;
            if (go.activeSelf != active)
                go.SetActive(active);
        }
        
        /// <summary>
        /// Disable GameObject if it's already active and Activate again
        /// </summary>
        /// <param name="go">GameObject</param>
        public static void ReactivateSafe(this GameObject go)
        {
            if (go == null) return;
            
            if (go.activeInHierarchy)
                go.SetActive(false);
            
            go.SetActive(true);
        }
        
        /// <summary>
        /// Deactivate GameObject if it's already active and vice versa 
        /// </summary>
        /// <param name="go"></param>
        /// <param name="isActive"></param>
        public static void ToggleActiveSafe(this GameObject go, bool isActive)
        {
            if (go == null) return;

            if (go.activeInHierarchy != isActive)
            {
                go.SetActive(isActive);
            }
        }

        public static void SetColor(this LineRenderer lr, Color color, float alpha = -1)
        {
            if (alpha > 0)
            {
                color.a = alpha;
            }

            lr.startColor = color;
            lr.endColor = color;
        }

        public static bool HasParameterOfType(this Animator animator, string name, AnimatorControllerParameterType type)
        {
            var parameters = animator.parameters;
            foreach (var currParam in parameters)
                if (currParam.type == type && currParam.name == name)
                    return true;
            return false;
        }

        public static void SetTriggerIfAvailable(this Animator animator, string name)
        {
            if (animator.HasParameterOfType(name, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(name);
        }

        public static void SetFloatIfAvailable(this Animator animator, string name, float value)
        {
            if (animator.HasParameterOfType(name, AnimatorControllerParameterType.Float))
                animator.SetFloat(name, value);
        }

        public static void ResetTriggerIfAvailable(this Animator animator, string name)
        {
            if (animator.HasParameterOfType(name, AnimatorControllerParameterType.Trigger))
            {
                animator.ResetTrigger(name);
                animator.SetTrigger(name);
            }
        }

        public static void SetBoolIfAvailable(this Animator animator, string name, bool b)
        {
            if (animator.HasParameterOfType(name, AnimatorControllerParameterType.Bool))
                animator.SetBool(name, b);
        }
        
        public static void StopCoroutines(this MonoBehaviour beh, params Coroutine[] coroutines)
        {
            foreach (var c in coroutines)
                if (c != null)
                    beh.StopCoroutine(c);
        }

        public static AnimationCurve OffsetInnerKeys(this AnimationCurve source, float offset)
        {
            var curve = new AnimationCurve(source.keys);
            for (int i = 1; i < curve.keys.Length - 1; i++)
            {
                var newKey = curve.keys[i];
                newKey.value += offset;
                curve.MoveKey(i, newKey);
            }
            return curve;
        }

        public static void DestroyChildren(this Transform transform, params Transform[] exceptions)
        {
            foreach (Transform t in transform)
            {
                if (exceptions.Contains(t))
                    continue;
                if (Application.isPlaying)
                    Object.Destroy(t.gameObject);
                else
                    Object.DestroyImmediate(t.gameObject);
            }
        }

        #endregion

    }
}
