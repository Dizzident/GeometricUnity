# Phase LIX - W/Z Absolute Mass Observable Projector

Phase LVIII added the calibration builder. Phase LIX adds the observable
projector that applies a validated shared scale to validated W/Z internal modes.

`WzAbsoluteMassObservableProjector` emits:

- `physical-w-boson-mass-gev`;
- `physical-z-boson-mass-gev`.

It propagates internal-mode and scale uncertainty in quadrature. It blocks when
the calibration build is not validated or when either validated W/Z internal
mode is missing.

The missing scientific input remains the electroweak bridge record.
