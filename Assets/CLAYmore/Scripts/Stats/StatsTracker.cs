using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Tracks player statistics across sessions and persists them via PlayerPrefs.
    ///
    /// Scene setup: add to any persistent GameObject, assign playerHealth and economy in the Inspector.
    /// </summary>
    public class StatsTracker : MonoBehaviour
    {
        private GameConfig _config;

        public void Init(GameConfig config) => _config = config;

        // ── Public API (read by UI) ───────────────────────────────────────────

        /// <summary>Total seconds the player has spent in-game (cumulative, cross-session).</summary>
        public float TotalTimePlayed { get; private set; }

        /// <summary>Total pots destroyed (all sessions).</summary>
        public int PotsDestroyed { get; private set; }

        /// <summary>Total steps walked (all sessions).</summary>
        public int TilesWalked { get; private set; }

        /// <summary>Total modifiers chosen (all sessions).</summary>
        public int ModifiersChosen { get; private set; }

        /// <summary>Total coins earned (all sessions).</summary>
        public int CoinsEarned { get; private set; }

        /// <summary>Score calculated at the end of the last game session.</summary>
        public int LastScore { get; private set; }

        // ── PlayerPrefs keys ──────────────────────────────────────────────────

        private const string KeyTime      = "stats_time";
        private const string KeyPots      = "stats_pots";
        private const string KeyTiles     = "stats_tiles";
        private const string KeyModifiers = "stats_modifiers";
        private const string KeyCoins     = "stats_coins";

        // ── Internal state ────────────────────────────────────────────────────

        private bool _isPaused;

        // Per-session counters (reset on restart, used for score calculation)
        private float _sessionTime;
        private int   _sessionPots;
        private int   _sessionModifiers;
        private int   _sessionCoins;

        public float SessionTimePlayed => _sessionTime;
        public int   SessionPots       => _sessionPots;
        public int   SessionModifiers  => _sessionModifiers;
        public int   SessionCoins      => _sessionCoins;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            Load();
        }

        private void OnEnable()
        {
            var bus = World.Current?.Events;
            if (bus != null)
            {
                bus.Subscribe<TilePotStateChangedEvent>(OnPotStateChanged);
                bus.Subscribe<PlayerMoveResultEvent>(OnPlayerMoveResult);
                bus.Subscribe<ModifierChosenEvent>(OnModifierChosen);
                bus.Subscribe<GamePausedEvent>(OnGamePaused);
                bus.Subscribe<GameOverEvent>(OnGameOver);
                bus.Subscribe<CoinsAddedEvent>(OnCoinsAdded);
                bus.Subscribe<GameRestartedEvent>(OnGameRestarted);
            }
        }

        private void OnDisable()
        {
            var bus = World.Current?.Events;
            if (bus != null)
            {
                bus.Unsubscribe<TilePotStateChangedEvent>(OnPotStateChanged);
                bus.Unsubscribe<PlayerMoveResultEvent>(OnPlayerMoveResult);
                bus.Unsubscribe<ModifierChosenEvent>(OnModifierChosen);
                bus.Unsubscribe<GamePausedEvent>(OnGamePaused);
                bus.Unsubscribe<GameOverEvent>(OnGameOver);
                bus.Unsubscribe<CoinsAddedEvent>(OnCoinsAdded);
                bus.Unsubscribe<GameRestartedEvent>(OnGameRestarted);
            }

            Save();
        }

        private void Update()
        {
            if (!_isPaused)
            {
                TotalTimePlayed += Time.deltaTime;
                _sessionTime    += Time.deltaTime;
            }
        }

        private void OnApplicationQuit() => Save();

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnPotStateChanged(TilePotStateChangedEvent e)
        {
            if (e.OldState == CellState.HasPot && e.NewState == CellState.Empty)
            {
                PotsDestroyed++;
                _sessionPots++;
                Save();
            }
        }

        private void OnPlayerMoveResult(PlayerMoveResultEvent e)
        {
            if (e.MoveType == MoveType.Walk)
                TilesWalked++;
        }

        private void OnModifierChosen(ModifierChosenEvent e)
        {
            ModifiersChosen++;
            _sessionModifiers++;
            Save();
        }

        private void OnGamePaused(GamePausedEvent e)
        {
            _isPaused = e.IsPaused;
            if (_isPaused)
            {
                Save();
                LogStats("ПАУЗА");
            }
        }

        private void OnGameOver(GameOverEvent e)
        {
            LastScore = CalculateScore();
            Save();
            LogStats("GAME OVER");
            LogScore();
            World.Current?.Events.Publish(new SessionScoredEvent { Score = LastScore });
        }

        private void OnGameRestarted(GameRestartedEvent e)
        {
            ResetSession();
            Save();
        }

        private void OnCoinsAdded(CoinsAddedEvent e)
        {
            CoinsEarned    += e.Amount;
            _sessionCoins  += e.Amount;
        }

        // ── Scoring ───────────────────────────────────────────────────────────

        public int CalculateScore()
        {
            if (_config == null) return 0;
            return (int)(_sessionTime      * _config.pointsPerSecond)
                 + _sessionPots            * _config.pointsPerPot
                 + _sessionModifiers       * _config.pointsPerModifier
                 + _sessionCoins           * _config.pointsPerCoin;
        }

        private void ResetSession()
        {
            _sessionTime      = 0f;
            _sessionPots      = 0;
            _sessionModifiers = 0;
            _sessionCoins     = 0;
        }

        // ── Logging ───────────────────────────────────────────────────────────

        private void LogStats(string reason)
        {
            int minutes = (int)(TotalTimePlayed / 60f);
            int seconds = (int)(TotalTimePlayed % 60f);
            Debug.Log(
                $"[StatsTracker] {reason}\n" +
                $"  Время в игре:      {minutes:00}:{seconds:00}\n" +
                $"  Горшков разбито:   {PotsDestroyed}\n" +
                $"  Шагов сделано:     {TilesWalked}\n" +
                $"  Монет заработано:  {CoinsEarned}\n" +
                $"  Модификаторов:     {ModifiersChosen}"
            );
        }

        private void LogScore()
        {
            if (_config == null) return;
            int sesMin = (int)(_sessionTime / 60f);
            int sesSec = (int)(_sessionTime % 60f);
            Debug.Log(
                $"[StatsTracker] ИТОГ СЕССИИ\n" +
                $"  Время:        {sesMin:00}:{sesSec:00}  × {_config.pointsPerSecond}/сек  = {(int)(_sessionTime * _config.pointsPerSecond)}\n" +
                $"  Горшков:      {_sessionPots}  × {_config.pointsPerPot}  = {_sessionPots * _config.pointsPerPot}\n" +
                $"  Монет:        {_sessionCoins}  × {_config.pointsPerCoin}  = {_sessionCoins * _config.pointsPerCoin}\n" +
                $"  Модификаторов:{_sessionModifiers}  × {_config.pointsPerModifier}  = {_sessionModifiers * _config.pointsPerModifier}\n" +
                $"  ──────────────────────────\n" +
                $"  ОЧКОВ ИТОГО:  {LastScore}"
            );
        }

        // ── Persistence ───────────────────────────────────────────────────────

        private void Load()
        {
            TotalTimePlayed = PlayerPrefs.GetFloat(KeyTime,      0f);
            PotsDestroyed   = PlayerPrefs.GetInt  (KeyPots,      0);
            TilesWalked     = PlayerPrefs.GetInt  (KeyTiles,     0);
            ModifiersChosen = PlayerPrefs.GetInt  (KeyModifiers, 0);
            CoinsEarned     = PlayerPrefs.GetInt  (KeyCoins,     0);
        }

        private void Save()
        {
            PlayerPrefs.SetFloat(KeyTime,      TotalTimePlayed);
            PlayerPrefs.SetInt  (KeyPots,      PotsDestroyed);
            PlayerPrefs.SetInt  (KeyTiles,     TilesWalked);
            PlayerPrefs.SetInt  (KeyModifiers, ModifiersChosen);
            PlayerPrefs.SetInt  (KeyCoins,     CoinsEarned);
            PlayerPrefs.Save();
        }

        /// <summary>Resets cumulative stats to zero and clears saved data.</summary>
        public void ResetAll()
        {
            TotalTimePlayed = 0f;
            PotsDestroyed   = 0;
            TilesWalked     = 0;
            ModifiersChosen = 0;
            CoinsEarned     = 0;
            LastScore       = 0;
            Save();
        }
    }
}
