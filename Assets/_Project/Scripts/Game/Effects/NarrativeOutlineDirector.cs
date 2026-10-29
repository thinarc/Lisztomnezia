using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Effects
{
    public static class NarrativeOutlineDirector
    {
        /*
         * РИСОВАНИЕ КРОЛИКА (начало)
         * лёгкая тревога, фон
         */
        public static void StartRabbitSketch()
        {
            OutlineMover.SetColor(OutlineMover.ColorMode.Cold);
            OutlineMover.SetBreath(OutlineMover.ColorBreath.Soft);
            OutlineMover.SetScenario(OutlineMover.OutlineScenario.SlowPressure);
        }

        /*
         * ПРОЦЕСС РИСОВАНИЯ (усиление)
         */
        public static void IntensifyRabbitSketch()
        {
            OutlineMover.SetBreath(OutlineMover.ColorBreath.Medium);
            OutlineMover.SetScenario(OutlineMover.OutlineScenario.AmbientNoise);
        }

        /*
         * ЗАВЕРШИЛ РИСУНОК (резкий триггер)
         */
        public static async UniTask FinishRabbitSketch()
        {
            OutlineMover.SetColor(OutlineMover.ColorMode.Horror);
            OutlineMover.SetScenario(OutlineMover.OutlineScenario.HorrorFlash);

            await UniTask.Delay(400);

            OutlineMover.SetScenario(OutlineMover.OutlineScenario.StressPulse);
        }

        /*
         * УБИЙСТВО (отдельное событие)
         */
        public static async UniTask KillEvent()
        {
            OutlineMover.SetColor(OutlineMover.ColorMode.Horror);
            OutlineMover.SetBreath(OutlineMover.ColorBreath.Strong);

            OutlineMover.SetScenario(OutlineMover.OutlineScenario.HorrorFlash);

            await UniTask.Delay(350);

            OutlineMover.SetScenario(OutlineMover.OutlineScenario.StressPulse);
        }

        /*
         * ВОЗВРАТ В СПОКОЙСТВИЕ
         */
        public static void ReturnToCalm()
        {
            OutlineMover.SetColor(OutlineMover.ColorMode.Base);
            OutlineMover.SetBreath(OutlineMover.ColorBreath.None);
            OutlineMover.SetScenario(OutlineMover.OutlineScenario.CalmBreath);
        }

        /*
         * ВСТРЕЧА ДЕВУШКИ
         */
        public static void GirlEncounterStart()
        {
            OutlineMover.SetColor(OutlineMover.ColorMode.Warm);
            OutlineMover.SetBreath(OutlineMover.ColorBreath.Soft);
            OutlineMover.SetScenario(OutlineMover.OutlineScenario.CalmBreath);
        }

        /*
         * ОБЩЕНИЕ (мягкое состояние)
         */
        public static void GirlConversation()
        {
            OutlineMover.SetColor(OutlineMover.ColorMode.Dream);
            OutlineMover.SetBreath(OutlineMover.ColorBreath.Soft);
        }

        /*
         * ПОДГОТОВКА К ИСПЫТАНИЮ
         */
        public static void GirlChallengePrepare()
        {
            OutlineMover.SetColor(OutlineMover.ColorMode.Cold);
            OutlineMover.SetBreath(OutlineMover.ColorBreath.Medium);
            OutlineMover.SetScenario(OutlineMover.OutlineScenario.SlowPressure);
        }

        /*
         * НАПРЯЖЕНИЕ ПЕРЕД СТАРТОМ
         */
        public static void GirlChallengeTension()
        {
            OutlineMover.SetScenario(OutlineMover.OutlineScenario.StressPulse);
        }

        /*
         * СТАРТ ИСПЫТАНИЯ
         */
        public static void GirlChallengeStart()
        {
            OutlineMover.SetScenario(OutlineMover.OutlineScenario.OnOsu);
        }
    }
}