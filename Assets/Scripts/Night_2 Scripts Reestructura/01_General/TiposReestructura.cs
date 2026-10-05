// ============================================================
// NOCHE 2 REESTRUCTURADA — Tipos (enums) compartidos por los scripts "...Reestructura".
// Este archivo NO se agrega a ningún GameObject: solo define listas de opciones
// que aparecen como menús desplegables en el Inspector.
// ============================================================

/// <summary>Las tres tareas de limpieza del inicio de la Noche 2.</summary>
public enum TareaReestructura
{
    Barrer,     // Tarea 1: barrer los montículos del bar
    Barra,      // Tarea 2: limpiar la barra y el exhibidor
    Banos       // Tarea 3: limpiar los inodoros de los baños
}

/// <summary>Herramienta que se necesita para limpiar una zona.</summary>
public enum HerramientaReestructura
{
    Automatica, // Barrer → Escoba, Barra → Trapo, Baños → Cepillo
    Ninguna,
    Escoba,
    Trapo,
    Cepillo
}

/// <summary>Qué hace un TriggerZonaReestructura cuando el jugador lo atraviesa.</summary>
public enum TipoTriggerReestructura
{
    ParpadeoSalidaBanos,    // Entrada del pasillito de los baños → Parpadeo N°1 y comienzo del servicio
    SilenciarGolpesSotano,  // Cerca de la puerta del sótano → los golpes se apagan
    FrenoEscaleraSotano     // Primer escalón del sótano → Lucas se frena y vuelve hacia atrás
}

/// <summary>Fase desde la que arranca la Noche 2 al darle Play (solo para testear).</summary>
public enum FaseDebugReestructura
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

/// <summary>Qué hace cada puerta de cubículo con física (ControlPuertaFisicaReestructura).</summary>
public enum RolPuertaReestructura
{
    FueraDeServicio,  // Tercer cubículo: trabada en las tareas, "cede" y se abre lento después de la nota
    SeCierraDeGolpe   // Cubículos de al lado: se abren al volver al baño y se cierran de golpe con la llave
}
