using UnityEngine;

public class SC_pulse : MonoBehaviour
{
    public ParticleSystem[] targetParticleSystems; // Tablica docelowych systemów cząsteczkowych
    public float PulseDuration = 1.0f; // Czas trwania pulsowania w sekundach
    public float EmissionIncreasePercentage = 50f; // Procent zwiększenia emisji

    private float[] defaultEmissionRates; // Tablica domyślnych wartości emisji
    private bool isPulsing = false; // Flaga informująca, czy pulsowanie jest aktywne
    private float pulseTime = 0f; // Licznik czasu pulsowania

    void Start()
    {
        // Inicjalizujemy tablicę domyślnych wartości emisji
        defaultEmissionRates = new float[targetParticleSystems.Length];

        // Pobieramy domyślne wartości emisji dla każdego systemu cząsteczkowego
        for (int i = 0; i < targetParticleSystems.Length; i++)
        {
            if (targetParticleSystems[i] != null)
            {
                var emission = targetParticleSystems[i].emission;
                defaultEmissionRates[i] = emission.rateOverTime.constant;
            }
        }
    }

    void Update()
    {
        if (isPulsing)
        {
            // Aktualizujemy licznik czasu pulsowania
            pulseTime += Time.deltaTime;

            // Obliczamy współczynnik mieszania emisji
            float t = Mathf.Clamp01(pulseTime / PulseDuration);

            // Ustawiamy nową wartość emisji na każdym systemie cząsteczkowym
            for (int i = 0; i < targetParticleSystems.Length; i++)
            {
                if (targetParticleSystems[i] != null)
                {
                    var emission = targetParticleSystems[i].emission;
                    float increasedRate = defaultEmissionRates[i] * (1 + EmissionIncreasePercentage / 100);
                    emission.rateOverTime = Mathf.Lerp(increasedRate, defaultEmissionRates[i], t);
                }
            }

            // Kończymy pulsowanie, gdy czas pulsowania minie
            if (pulseTime >= PulseDuration)
            {
                isPulsing = false;
                for (int i = 0; i < targetParticleSystems.Length; i++)
                {
                    if (targetParticleSystems[i] != null)
                    {
                        var emission = targetParticleSystems[i].emission;
                        emission.rateOverTime = defaultEmissionRates[i];
                    }
                }
            }
        }
    }

    public void MakePulse()
    {
        // Ustawiamy zwiększoną emisję na każdym systemie cząsteczkowym
        for (int i = 0; i < targetParticleSystems.Length; i++)
        {
            if (targetParticleSystems[i] != null)
            {
                var emission = targetParticleSystems[i].emission;
                emission.rateOverTime = defaultEmissionRates[i] * (1 + EmissionIncreasePercentage / 100);
            }
        }
        isPulsing = true;
        pulseTime = 0f;
    }
}
