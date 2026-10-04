// ============================================================
// NOCHE 2 (reestructura) — Tipos compartidos por los scripts nuevos.
// ============================================================

/// <summary>Las tres tareas de limpieza del inicio de la Noche 2.</summary>
public enum TareaAct2
{
    Barrer,     // Tarea 1: barrer los montículos del bar
    Barra,      // Tarea 2: limpiar la barra y el exhibidor
    Banos       // Tarea 3: limpiar los inodoros de los baños
}

/// <summary>Herramienta que se necesita para limpiar una zona.</summary>
public enum HerramientaAct2
{
    Automatica, // Barrer → Escoba, Barra → Trapo, Baños → Cepillo
    Ninguna,
    Escoba,
    Trapo,
    Cepillo
}

/// <summary>Qué hace un TriggerZonaAct2 cuando el jugador lo atraviesa.</summary>
public enum TipoTriggerAct2
{
    ParpadeoSalidaBanos,    // Entrada del pasillito de los baños → Parpadeo N°1 y comienzo del servicio
    SilenciarGolpesSotano,  // Cerca de la puerta del sótano → los golpes se apagan
    FrenoEscaleraSotano     // Primer escalón del sótano → Lucas se frena y vuelve hacia atrás
}

/// <summary>Fase desde la que arranca la Noche 2 (solo para testear).</summary>
public enum FaseDebugAct2
{
    Normal,             // Juego completo desde la intro
    Tareas,
    Servicio,
    ClienteCorrupto,
    Pasillo,
    PuertaSotano,
    RegresoBano,
    Vigilante,
    Combate,
    Cierre
}
