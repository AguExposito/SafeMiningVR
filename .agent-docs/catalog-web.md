# Catálogo de simulaciones para la web

Un solo JSON describe las simulaciones. El casco y la web leen el mismo archivo. El acceso por empresa sigue en el backend.

## Repo y URL

| | |
|---|---|
| Repositorio | https://github.com/AguExposito/SafeMiningVR |
| Rama | `main` |
| Archivo | `Assets/Resources/simulations.json` |
| URL | https://raw.githubusercontent.com/AguExposito/SafeMiningVR/main/Assets/Resources/simulations.json |

El repo es público. La respuesta incluye `Access-Control-Allow-Origin: *`, así que el navegador puede hacer `fetch` directo.

```js
const url = "https://raw.githubusercontent.com/AguExposito/SafeMiningVR/main/Assets/Resources/simulations.json";
const catalog = await (await fetch(url)).json();
```

## Qué usar

- `catalog.simulations` es la lista. Una ficha por elemento.
- `id` es el identificador estable. LOTO es `loto_simulation`.
- `implemented: true` solo en la simulación que el casco puede jugar. El resto es ficha: el casco no abre escena y ofrece **Simular score**.
- `Simular score` usa el mismo camino que terminar LOTO: EnqueueComplete → cola → SafeMiningBackendAdapter.CompleteSessionAsync → POST /api/v1/result/{id} (el id de esa sim, p. ej. sim_prueba). El POST sale solo si hay login JWT Online y el adapter está disponible. Si no, queda en PendingSyncQueue.json y se reintenta después.
- `phases` (`epp`, `limpieza`, `loto`) y `objectives` son los datos de puntaje. `approvalThreshold` es el umbral 0–100.
- Subir `version` cuando cambie el contenido.

No hay empresas ni permisos en este archivo. Dar de alta una simulación es editar el JSON y hacer push a `main`.


```js
JSON real no admite comentarios; esto es una guía (estilo JSONC). El archivo en el repo debe ir sin //.
{
  // Subí este número cada vez que cambies el contenido.
  // El casco elige la versión más alta entre Resources (build) y la caché local.
  "version": 1,

  // Lista de simulaciones. Una tarjeta en el carrusel del casco = un elemento.
  // La web debe hacer GET del mismo archivo y listar este arreglo.
  "simulations": [

    // ---------- SIMULACIÓN REAL (jugable en VR) ----------
    {
      // Id estable. Es el {id} del POST /api/v1/result/{id}.
      // LOTO DEBE ser exactamente "loto_simulation" (hardcode en el casco).
      "id": "loto_simulation",

      // Nombre visible en el carrusel / catálogo web.
      "name": "Inspeccion Pre-Uso de EPP y Bloqueo y Etiquetado (LOTO)",

      // Informativo: "vr" | "web" | lo que acuerden. El casco hoy no filtra por esto.
      "platform": "vr",

      // true  → botón JUGAR (solo tiene circuito 3D la de LOTO).
      // false → botón SIMULAR SCORE (no abre escena).
      // Hoy el casco SOLO abre escena si id == "loto_simulation".
      // Aunque pongas implemented:true en otra, no la juega.
      "implemented": true,

      // Umbral de aprobación 0–100 (RN02). Afecta LOTO vía catálogo.
      "approvalThreshold": 60,

      // Fases → body del API (VrResultSubmitBody).
      // apiKey SOLO reconocidas al simular / mapear: "epp" | "limpieza" | "loto"
      // Otras apiKey se ignoran en el casco (hoy).
      "phases": [
        {
          "id": "loto_phase_s1",   // id interno; en LOTO conviene mantener estos
          "apiKey": "epp",         // campo JSON del POST: epp
          "weight": 0.3,           // peso de fase (base 1). Sumar ~1.0 en LOTO
          "threshold": 60          // umbral por fase (base 100)
        },
        {
          "id": "loto_phase_s2",
          "apiKey": "limpieza",
          "weight": 0.25,
          "threshold": 60
        },
        {
          "id": "loto_phase_s3",
          "apiKey": "loto",
          "weight": 0.45,
          "threshold": 60
        }
      ],

      // Objetivos (lobby / pesos legacy). En LOTO los ids deben coincidir
      // con LotoObjectiveIds (loto_obj_*). weightPercent suma 100 idealmente.
      "objectives": [
        { "id": "loto_obj_epp_selection", "weightPercent": 20, "phaseId": "loto_phase_s1" },
        { "id": "loto_obj_epp_defect",    "weightPercent": 10, "phaseId": "loto_phase_s1" },
        { "id": "loto_obj_hazard_report", "weightPercent": 25, "phaseId": "loto_phase_s2" },
        { "id": "loto_obj_loto_sequence", "weightPercent": 35, "phaseId": "loto_phase_s3" },
        { "id": "loto_obj_zero_energy",   "weightPercent": 10, "phaseId": "loto_phase_s3" }
      ]
    },

    // ---------- SIMULACIÓN DE PRUEBA / FICHA (no jugable) ----------
    {
      // Id libre (snake_case). Será el path del POST al simular score.
      // Ejemplo: POST .../api/v1/result/sim_prueba
      // El backend debe conocer ese id si querés que el POST no dé 404.
      "id": "sim_prueba",

      "name": "Simulacion de prueba",
      "platform": "vr",
      "implemented": false,   // ficha: carrusel muestra SIMULAR SCORE
      "approvalThreshold": 60,

      // Opcional. Vacío → al simular solo rellena epp (fallback).
      // Con fases, usa apiKey epp/limpieza/loto y genera scores random.
      "phases": [
        // { "id": "prueba_p1", "apiKey": "epp", "weight": 0.5, "threshold": 60 },
        // { "id": "prueba_p2", "apiKey": "limpieza", "weight": 0.5, "threshold": 60 }
      ],

      // Objetivos de catálogo/web. En ficha el casco solo muestra el conteo.
      // phaseId puede ir "" si no aplica.
      "objectives": [
        { "id": "prueba_obj_1", "weightPercent": 100, "phaseId": "" }
      ]
    }
  ]
}
```