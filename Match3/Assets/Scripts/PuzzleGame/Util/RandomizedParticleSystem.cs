using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGame.Util.Pool;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PuzzleGame.Util
{
    [Serializable]
    internal class SeedGroup
    {
        public ParticleSystem[] particleSystems;
    }

    [Serializable]
    internal class AlternativeGroup
    {
        public ParticleSystem[] particleSystems;
    }

    public class RandomizedParticleSystem : MonoBehaviour, IPoolable
    {
        [SerializeField] private ParticleSystem targetParticleSystem;
        [SerializeField] private ParticleSystem[] additionalParticleSystems;

        [SerializeField, BoxGroup("Synced Seed"),
         Tooltip("Will set the seed of all particle systems in the seed groups to a random value between the range.")]
        private bool syncSeeds;

        [SerializeField, BoxGroup("Synced Seed"), ShowIf(nameof(syncSeeds))]
        private Vector2Int seedRange = new Vector2Int(-2147483647, 2147483647);

        [SerializeField, BoxGroup("Synced Seed"), ShowIf(nameof(syncSeeds))]
        private SeedGroup[] seedGroups;

        [SerializeField, BoxGroup("Alternatives")]
        private bool randomizeAlternatives;

        [SerializeField, BoxGroup("Alternatives"), ShowIf(nameof(randomizeAlternatives))]
        private AlternativeGroup[] alternativeGroups;

        [SerializeField] private List<ParticleSizeData> particleSizeDataList;

        public ParticleSystem Settings => targetParticleSystem;

        [BoxGroup("Debug"), Button(ButtonSizes.Medium), GUIColor(0.8f, 0.8f, 0.8f)]
        public void Play(ParticleSize size = ParticleSize.Medium)
        {
            targetParticleSystem.transform.localScale =
                Vector3.one * particleSizeDataList.First(p => p.ParticleSize == size).SizeMultiplier;

            if (syncSeeds)
            {
                Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                foreach (var seedGroup in seedGroups)
                {
                    var seed = UnityEngine.Random.Range(seedRange.x, seedRange.y);
                    foreach (var particle in seedGroup.particleSystems)
                    {
                        particle.useAutoRandomSeed = false;
                        particle.randomSeed = (uint)seed;
                    }
                }
            }

            if (randomizeAlternatives)
            {
                foreach (var alternativeGroup in alternativeGroups)
                {
                    var randomIndex = UnityEngine.Random.Range(0, alternativeGroup.particleSystems.Length);
                    for (var index = 0; index < alternativeGroup.particleSystems.Length; index++)
                    {
                        var particleObject = alternativeGroup.particleSystems[index].gameObject;
                        particleObject.SetActive(index == randomIndex);
                    }
                }
            }

            targetParticleSystem.Play();
        }

        public async UniTask WaitForStop(CancellationToken token)
        {
            await UniTask.NextFrame(token);
            while (true)
            {
                if (additionalParticleSystems.All(p => !p.isPlaying) &&
                    seedGroups.All(g => g.particleSystems.All(p => !p.isPlaying)))
                    break;
                await UniTask.Yield(token);
            }
        }

        public void Stop(bool withChildren = false,
            ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting)
        {
            targetParticleSystem.Stop(withChildren, stopBehavior);
        }

        [Button]
        public void SetColors(Color tileColor, GradientAlphaKey[] alphaKeys)
        {
            var newGradient = new Gradient();
            var colorKeys = new List<GradientColorKey>();
            colorKeys.Add(new GradientColorKey(tileColor, 0));
            colorKeys.Add(new GradientColorKey(tileColor, 1));
            var colorKeysArray = colorKeys.ToArray();
            newGradient.SetKeys(colorKeysArray, alphaKeys);
            SetColor(targetParticleSystem, tileColor, newGradient);
            foreach (var alternativeGroup in alternativeGroups)
            {
                foreach (var particle in alternativeGroup.particleSystems)
                {
                    SetColor(particle, tileColor, newGradient);
                }
            }

            foreach (var alternativeGroup in seedGroups)
            {
                foreach (var particle in alternativeGroup.particleSystems)
                {
                    SetColor(particle, tileColor, newGradient);
                }
            }

            foreach (var particle in additionalParticleSystems)
            {
                SetColor(particle, tileColor, newGradient);
            }
        }

        private void SetColor(ParticleSystem particle, Color tileColor, Gradient newGradient)
        {
            var mainModule = particle.main;
            mainModule.startColor = tileColor;
            var colorOverLifetimeModule = particle.colorOverLifetime;
            colorOverLifetimeModule.color = new ParticleSystem.MinMaxGradient(newGradient);
        }

        public void OnDespawn()
        {
            Stop();
        }

        public void OnSpawn()
        {
        }
    }

    [Serializable]
    public struct ParticleSizeData
    {
        [SerializeField] private ParticleSize particleSize;
        [SerializeField] private float sizeMultiplier;

        public ParticleSize ParticleSize => particleSize;
        public float SizeMultiplier => sizeMultiplier;
    }

    public enum ParticleSize
    {
        Small,
        Medium,
        Large
    }
}