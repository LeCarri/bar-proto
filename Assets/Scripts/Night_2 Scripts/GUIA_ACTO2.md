# GUÍA COMPLETA — ACTO 2: La Fisura de la Realidad

> Leé esta guía de arriba a abajo la primera vez. Después podés ir directo a la sección que necesitás.

---

## ÍNDICE
1. [Qué hace cada script](#1-qué-hace-cada-script)
2. [Jerarquía de la escena Night_2](#2-jerarquía-de-la-escena-night_2)
3. [Paso a paso: configurar cada GameObject](#3-paso-a-paso-configurar-cada-gameobject)
4. [Usar el Auto-Configurador](#4-usar-el-auto-configurador)
5. [Prefab de la Sombra enemiga](#5-prefab-de-la-sombra-enemiga)
6. [Herramientas de debug](#6-herramientas-de-debug)
7. [Errores comunes y cómo resolverlos](#7-errores-comunes-y-cómo-resolverlos)

---

## 1. QUÉ HACE CADA SCRIPT

| Script | Se pone en... | Qué hace |
|--------|--------------|----------|
| `Act2Manager` | GameObject vacío `--Acts--/Act2Manager` | **Cerebro del acto.** Controla el orden de todos los eventos. |
| `ParpadeoBarCambio` | GameObject vacío `--Acts--/EffectoParpadeo` | Hace parpadeos a negro y cambia elementos del bar en cada uno. |
| `ClienteCorrupto` | Cada NPC cliente del Acto 2 | Cliente deformado con voz distorsionada. Dispara la ida a la cocina. |
| `PasilloEfecto` | Box Collider trigger en la entrada del pasillo | Efecto de corredor infinito (expande el FOV de la cámara). |
| `BebidaEspecialAct2` | El objeto botella en la cocina | El jugador lo recoge para completar el pedido del cliente. |
| `ZapatosNino` | Modelo de zapatos en el estante de la cocina | Al examinarlos sube la paranoia y activa los golpes del sótano. |
| `NotaPuerta` | Objeto papel pegado en la puerta del sótano | Al leerla da la pista del baño y activa la llave. |
| `PuertaSotanoAct2` | El GameObject de la puerta del sótano | Reproduce golpes rítmicos, acepta la llave, se abre sola al final. |
| `LlaveInteractuable` | Objeto llave en el baño | Al recogerla activa el Vigilante en el espejo y la psicosis. |
| `VigenteMirror` | GameObject vacío cerca del espejo del baño | Muestra al Vigilante reflejado durante 2 segundos. |
| `EfectoPsicosis` | GameObject vacío `--Acts--/EfectoPsicosis` | Distorsiona el FOV, pulsa un overlay rojo/verde, amplifica consumo de batería. |
| `SombrasCombate` | GameObject vacío `--Acts--/SombrasCombate` | Spawnea las sombras, bloquea salidas durante el combate. |
| `SombraLookAt` | **Prefab de Sombra** (junto a EnemyCore_Act2) | La sombra mira lentamente al jugador antes de atacar. |
| `EnemyCore_Act2` | **Prefab de Sombra** | Recibe daño de la linterna, al morir notifica a Act2Manager. |
| `FiguraNino` | Modelo del niño en la escena (desactivado) | Aparece en una mesa, al acercarse el jugador desaparece y llama desde el sótano. |
| `Flashlight_Act2` | Prefab del jugador (reemplaza `Flashlight`) | Igual a la linterna del Acto 1 pero consume batería al doble. |
| `Act2DebugHelper` | Cualquier GameObject en la escena | HUD de debug con teclas para saltar estados. |

---

## 2. JERARQUÍA DE LA ESCENA NIGHT_2

Creá esta estructura en el **Hierarchy** de Unity. Los nombres con `--` son GameObjects vacíos organizadores.

```
Night_2 Scene
├── --Acts--
│   ├── Act2Manager              ← script: Act2Manager
│   ├── EffectoParpadeo          ← scripts: EffectoParpadeo + ParpadeoBarCambio
│   ├── EfectoPsicosis           ← script: EfectoPsicosis
│   └── SombrasCombate           ← script: SombrasCombate
│
├── --Triggers--
│   ├── TriggerPasillo           ← Box Collider (Is Trigger ✓) + PasilloEfecto
│   └── [otros triggers]
│
├── --ClientGroup--              ← grupo de clientes (empieza DESACTIVADO)
│   ├── ClienteCorrupto_1        ← script: ClienteCorrupto
│   └── ClienteCorrupto_2        ← script: ClienteCorrupto
│
├── --Kitchen--
│   ├── BebidaEspecial           ← script: BebidaEspecialAct2
│   └── Zapatos                  ← script: ZapatosNino
│
├── --Bathroom--
│   ├── Llave                    ← script: LlaveInteractuable (empieza DESACTIVADO)
│   └── EspejoVigente            ← script: VigenteMirror
│
├── --Basement--
│   ├── PuertaSotano             ← script: PuertaSotanoAct2
│   └── NotaPuerta               ← script: NotaPuerta (empieza DESACTIVADO)
│
├── FiguraNino                   ← script: FiguraNino (empieza DESACTIVADO)
│
├── --Enemies--
│   └── [puntos de spawn vacíos: Spawn_1, Spawn_2, Spawn_3]
│
├── --Lights--
│   ├── LucesNormales            ← luces normales (amarillas)
│   ├── LucesServicio            ← luces de servicio (violetas)
│   └── LucesPsicosis            ← luces de psicosis (rojas)
│
├── --AudioSource--
│   ├── AudioAmbient             ← AudioSource con sonido ambiente del bar
│   ├── AudioMusic               ← AudioSource con música
│   ├── AudioGolpes              ← AudioSource con golpes del sótano (loop)
│   ├── AudioCrack               ← AudioSource con sonido de llave partiéndose
│   └── AudioStatic              ← AudioSource con estática
│
├── --UI--
│   ├── Canvas
│   │   ├── PanelNegro           ← Image negra, CanvasGroup, alpha=0
│   │   └── TextoSubtitulos      ← TextMeshProUGUI para los diálogos
│   └── EventSystem
│
├── --Jugador--
│   └── [prefab del jugador del Acto 1, ajustado]
│
└── Act2DebugHelper              ← script: Act2DebugHelper
```

> **Importante:** Los nombres en `--Lights--` y `--AudioSource--` deben ser exactamente:
> `LucesNormales`, `LucesServicio`, `LucesPsicosis`, `AudioAmbient`, `AudioMusic`, etc.
> El auto-configurador los busca por nombre.

---

## 3. PASO A PASO: CONFIGURAR CADA GAMEOBJECT

### PASO 1 — Act2Manager

1. Creá un **GameObject vacío** dentro de `--Acts--`, llamalo `Act2Manager`
2. Arrastrá el script `Act2Manager` al Inspector
3. Todavía no asignés nada — el auto-configurador lo hace en el Paso 4
4. **Excepción:** Si tenés varios Canvas en la escena, asigná manualmente el `TextoSubtitulos`

---

### PASO 2 — EffectoParpadeo + ParpadeoBarCambio

1. Creá un GameObject vacío `EffectoParpadeo` dentro de `--Acts--`
2. Agregale el script `EffectoParpadeo` (del Acto 1, ya existe)
3. Agregale el script `ParpadeoBarCambio`

**Configurar ParpadeoBarCambio en el Inspector:**
- `Pantalla Negra` → arrastrá el GameObject `PanelNegro` del Canvas
- `Cambios Secuenciales` → Size: **3** (uno por cada parpadeo del inicio)
  - **Cambio 0** → `Objetos A Activar`: cuadros torcidos, manchas en el piso
  - **Cambio 1** → `Objetos A Activar`: más manchas o sillas desplazadas
  - **Cambio 2** → `Objetos A Activar`: sillas muy desplazadas, objetos caídos
  - En `Objetos A Desactivar` ponés las versiones "normales" de esos mismos objetos

> Si no tenés los objetos aún, dejá los arrays vacíos — el parpadeo igual funciona, solo no cambia nada visualmente.

---

### PASO 3 — EfectoPsicosis

1. Creá un GameObject vacío `EfectoPsicosis` dentro de `--Acts--`
2. Agregale el script `EfectoPsicosis`

**Inspector:**
- `Camara Principal` → se asigna automáticamente si es null (busca `Camera.main`)
- `Overlay Psicosis` → creá un Image en el Canvas, color `(255, 0, 50, 90)` aprox rojo semitransparente, agregale un `CanvasGroup`, y arrastralo acá
- `Sonido Estatica` → arrastrá `AudioStatic`
- `Linterna` → si usás `Flashlight_Act2`, arrastrala acá para que duplique el consumo durante la psicosis. Si no usás esa linterna, dejalo vacío.

---

### PASO 4 — SombrasCombate

1. Creá un GameObject vacío `SombrasCombate` dentro de `--Acts--`
2. Agregale el script `SombrasCombate`

**Inspector:**
- `Prefab Sombra` → arrastrá el prefab de la sombra (configurado en el Paso 8)
- `Puntos De Sombra` → Size: **3**, arrastrá los GameObjects `Spawn_1`, `Spawn_2`, `Spawn_3` (posicionados cerca de las salidas del bar)
- `Bloqueadores Salidas` → Size: N, arrastrá los colliders invisibles que tapan las puertas
- `Delay Entre Sombras` → `0.6` segundos (podés ajustarlo)
- `Ambience Combate` → arrastrá un AudioSource con sonido de combate/tensión

---

### PASO 5 — Triggers del pasillo

1. Creá un GameObject vacío `TriggerPasillo` dentro de `--Triggers--`
2. Agregale un **Box Collider** → marcá **Is Trigger** ✓
3. Ajustá el tamaño para que cubra la entrada/el corredor hacia la cocina
4. Agregale el script `PasilloEfecto`

**Inspector:**
- `Camara Principal` → se asigna automáticamente (Camera.main)
- `Fov Normal` → `60` (el default de la cámara)
- `Fov Maximo` → `90` (máximo del efecto)
- `Duracion Efecto` → `8` segundos

---

### PASO 6 — Cocina: Bebida y Zapatos

**BebidaEspecialAct2:**
1. Seleccioná el GameObject de la botella/bebida en la cocina
2. Agregale el script `BebidaEspecialAct2`
3. Inspector: `Pasillo Efecto` → arrastrá el TriggerPasillo (para desactivar el efecto al llegar)

**ZapatosNino:**
1. Seleccioná el GameObject de los zapatos en el estante
2. Agregale el script `ZapatosNino`
3. Inspector: `Paranoia Al Encontrar` → `30` (lleva la paranoia a la mitad)

---

### PASO 7 — Puerta del Sótano

1. Seleccioná el GameObject de la puerta del sótano
2. Agregale el script `PuertaSotanoAct2`

**Inspector:**
- `Sonido Golpes Ritmicos` → arrastrá `AudioGolpes` (con un clip de golpes en loop)
- `Sonido Cerradura` → arrastrá `AudioCrack`
- Si la puerta tiene Animator: arrastrá el `Animator` y creá un trigger llamado `"Abrir"` en el Animator Controller
- Si NO tiene Animator: dejá el campo vacío y ajustá `Angulo Apertura` a `90` — la puerta rotará sola

---

### PASO 8 — Nota en la puerta

1. Creá un objeto plano (un Quad o Sprite) pegado visualmente sobre la puerta del sótano
2. Agregale el script `NotaPuerta`
3. **Dejalo DESACTIVADO** en la escena — Act2Manager lo activa cuando corresponde
4. Inspector: `Texto Nota` → `"La guardé donde nadie limpia."`

---

### PASO 9 — Baño: Llave

1. Posicioná un objeto pequeño (modelo de llave) en el baño, en un rincón sucio
2. Agregale el script `LlaveInteractuable`
3. **Dejalo DESACTIVADO** — Act2Manager lo activa cuando el jugador lee la nota

---

### PASO 10 — Baño: El Vigilante en el espejo

1. Tomá el modelo/prefab del Vigilante (de `PrototypePrefabs/EnemyPrefabs/Watchmen.prefab`)
2. Posicionalo **dentro o detrás del espejo**, como si fuera el reflejo
3. **Dejalo DESACTIVADO**
4. Creá un **GameObject vacío** cerca del espejo llamado `EspejoVigente`
5. Agregale el script `VigenteMirror`

**Inspector:**
- `Modelo Vigilante` → arrastrá el prefab del Vigilante posicionado en el espejo
- `Sacudida Camara` → arrastrá el `CameraShake` del jugador
- `Duracion Aparicion` → `2.2` segundos
- `Delay Antes Susto` → `0.8` segundos

---

### PASO 11 — Clientes Corruptos

1. Duplicá los prefabs de clientes del Acto 1 (o creá nuevos)
2. **Eliminá** el componente `ClienteInteractuable` si tiene uno
3. Agregale el script `ClienteCorrupto`
4. Poné todos los clientes dentro de `--ClientGroup--`
5. **Desactivá el grupo entero** — Act2Manager lo activa con el parpadeo

**Inspector (por cada cliente):**
- `Nombre Cliente` → `"???"`
- `Dialogo Pedido` → `"Servime lo más fuerte que tengas..."`
- `Pitch Distorsion` → `0.65` (voz grave y rara)
- `Voz Distorsionada` → un AudioSource hijo del cliente con pitch bajo
- Si es el cliente empapado: activale un sistema de partículas de agua/líquido

---

### PASO 12 — Figura del Niño

1. Tomá un modelo pequeño (puede ser un capsule temporalmente)
2. Posicionalo en la escena
3. **Desactivalo** — Act2Manager lo activa en el momento correcto
4. Agregale el script `FiguraNino`

**Inspector:**
- `Modelo Nino` → arrastrá el modelo hijo del GameObject
- `Posicion Mesa` → un Transform vacío posicionado sobre una mesa del bar
- `Voz Nino` → AudioSource con clip de voz del niño (puesto cerca del jugador)
- `Voz Sotano` → AudioSource **posicionado cerca de la puerta del sótano** con el mismo clip

---

### PASO 13 — UI: Canvas

El Canvas necesita exactamente **dos elementos**:

**PanelNegro:**
1. Creá un `UI → Image` llamado `PanelNegro`
2. Color: negro total `(0,0,0,255)`
3. Ajustalo para cubrir toda la pantalla (Anchor: stretch/stretch)
4. Agregale un `CanvasGroup`
5. `CanvasGroup.Alpha` → `0` al inicio
6. Desactivá **Interactable** y **Blocks Raycasts** en el CanvasGroup

**TextoSubtitulos:**
1. Creá un `UI → TextMeshPro - Text` llamado `TextoSubtitulos`
2. Anchorlo en la parte inferior (centro): `Anchor Min (0.1, 0.05)`, `Anchor Max (0.9, 0.2)`
3. `Font Size` → 28-32
4. Color: blanco
5. Alignment: Center/Middle

---

### PASO 14 — Debug Helper

1. Seleccioná cualquier GameObject (puede ser `Act2DebugHelper` vacío en la raíz)
2. Agregale el script `Act2DebugHelper`
3. `Mostrar Al Inicio` → ✓ (para que arranque visible mientras probás)

---

## 4. USAR EL AUTO-CONFIGURADOR

Una vez que tenés la jerarquía creada con los nombres correctos:

1. Seleccioná el GameObject `Act2Manager` en el Hierarchy
2. En el Inspector, **clic derecho** sobre el nombre del componente `Act2Manager`
3. Aparece un menú. Elegí → **"Auto-buscar referencias"**
4. Mirá la Consola de Unity — te dice cuántas referencias conectó
5. Revisá el Inspector: los campos que quedaron **vacíos** los tenés que asignar a mano

**El auto-configurador conecta automáticamente:**
- Efectos: EffectoParpadeo, ParpadeoBarCambio, EfectoPsicosis, CameraShake
- Luces: LucesNormales, LucesServicio, LucesPsicosis (por nombre exacto)
- UI: TextoSubtitulos, FadePanel/PanelNegro
- Scripts únicos: PasilloEfecto, PuertaSotanoAct2, NotaPuerta, LlaveInteractuable, VigenteMirror, SombrasCombate, FiguraNino
- AudioSources: busca dentro de `--AudioSource--` por palabras clave en el nombre

**Lo que SIEMPRE hay que asignar a mano:**
- `Grupo Clientes Corruptos` si el objeto no se llama exactamente `--ClientGroup--`
- Luces si sus nombres son distintos a `LucesNormales/LucesServicio/LucesPsicosis`

---

## 5. PREFAB DE LA SOMBRA ENEMIGA

El prefab de sombra necesita **3 componentes juntos**:

```
Sombra (Prefab)
├── EnemyCore_Act2          ← maneja la vida y muerte
├── SombraLookAt            ← fase de mirada antes de atacar
├── PersecutionEnemy        ← empieza DESHABILITADO, SombraLookAt lo activa
└── AudioSource             ← respiración (asignado en SombraLookAt)
```

**Pasos:**
1. Abrí el prefab `Shadows.prefab` desde `Assets/Prefabs/PrototypePrefabs/EnemyPrefabs/`
2. **Eliminá** el componente `EnemyCore` si tiene uno
3. Agregá `EnemyCore_Act2`
4. Agregá `SombraLookAt`
5. En `SombraLookAt`:
   - `Duracion Mirada` → `4` segundos
   - `Velocidad Rotacion` → `35` grados/seg (lento = más miedo)
   - `Componentes De Ataque` → arrastrá `PersecutionEnemy` (o el script de comportamiento que use)
   - `Audio Respiracion` → el AudioSource del prefab
6. Asegurate de que `PersecutionEnemy` esté **deshabilitado** al inicio (destilde el checkmark en el Inspector)
7. Verificá que el prefab esté en la **Layer correcta** para que la linterna lo detecte (`enemyLayer` en Flashlight_Act2)

---

## 6. HERRAMIENTAS DE DEBUG

### HUD en pantalla (Act2DebugHelper)

Mientras el juego corre:

| Tecla | Acción |
|-------|--------|
| `F1` | Mostrar/ocultar el HUD |
| `1` | Saltar al estado **Inicio** |
| `2` | Saltar al estado **Servicio** (clientes) |
| `3` | Saltar al estado **Pasillo** (corredor) |
| `4` | Saltar al estado **Sótano** (golpes + nota) |
| `5` | Saltar al estado **Baño** (llave) |
| `6` | Saltar al estado **Psicosis** (sombras) |
| `7` | Saltar al estado **Cierre** (final del acto) |
| `+` | Subir paranoia 20 puntos |
| `-` | Bajar paranoia 20 puntos |

### Clic derecho en Act2Manager (en Play o en Edit)

| Opción | Para qué sirve |
|--------|---------------|
| `⚠ Verificar escena` | Lista todos los campos nulos con error en la Consola |
| `▶ TEST: Mostrar diálogo de prueba` | Verifica que el texto aparece en pantalla |
| `▶ TEST: Saltar a SERVICIO` | Prueba solo la fase de clientes |
| `▶ TEST: Saltar a PSICOSIS` | Prueba solo el combate de sombras |
| `▶ TEST: Subir paranoia +30` | Activa los efectos de paranoia sin esperar |

### Leer la Consola

Todos los scripts tienen prefijo `[NombreScript]` en sus logs. Filtrá por:
- `[Act2Manager]` → errores de configuración y progresión del acto
- `[SombrasCombate]` → muertes de sombras
- `DIÁLOGO PERDIDO` → el texto de subtítulos no está asignado

---

## 7. ERRORES COMUNES Y CÓMO RESOLVERLOS

**"El diálogo no aparece en pantalla"**
→ Corré `⚠ Verificar escena`. Si `textoSubtitulos` es null, buscá `TextoSubtitulos` en el Canvas y arrastralo al campo.
→ Si no hay Canvas, Act2Manager crea uno automáticamente al iniciar (buscalo en el Hierarchy).

**"Los clientes no aparecen"**
→ Verificá que `--ClientGroup--` está en la escena con ese nombre exacto.
→ El grupo empieza desactivado — se activa solo en el estado Servicio.

**"La linterna no daña a las sombras"**
→ El prefab de la sombra necesita `EnemyCore_Act2` (no `EnemyCore`).
→ Verificá que la sombra está en la Layer asignada en `enemyLayer` de `Flashlight_Act2`.

**"La puerta del sótano no se abre"**
→ El jugador necesita tener la llave (recoger `LlaveInteractuable` en el baño primero).
→ Si el estado no llegó a `Bano`, usá el debug para saltar a ese estado y probarlo.

**"CS0111 — duplicate member"**
→ Hay dos archivos `.cs` con el mismo nombre de clase en Assets. Buscá con Ctrl+F en el Project panel y borrá el duplicado.

**"ACT1MANAGER DETECTADO" en la consola**
→ Hay un remanente del Acto 1 en la escena. Buscá `Act1Manager` en el Hierarchy y eliminalo.

**"La psicosis no termina aunque se derroten todas las sombras"**
→ Verificá `Total Sombras` en Act2Manager coincide con la cantidad real de puntos en `SombrasCombate`.

---

## CHECKLIST FINAL ANTES DE TESTEAR

- [ ] Act2Manager tiene el auto-setup ejecutado (`⚠ Verificar escena` sin errores rojos)
- [ ] `--ClientGroup--` desactivado al inicio
- [ ] `LlaveInteractuable` desactivado al inicio
- [ ] `FiguraNino` desactivado al inicio
- [ ] `NotaPuerta` desactivado al inicio
- [ ] Prefab de Sombra tiene `EnemyCore_Act2` + `SombraLookAt` + `PersecutionEnemy` (deshabilitado)
- [ ] La Layer de la Sombra coincide con `enemyLayer` de la linterna
- [ ] Canvas tiene `PanelNegro` (con CanvasGroup, alpha=0) y `TextoSubtitulos`
- [ ] `Act2DebugHelper` en la escena para poder saltar estados
