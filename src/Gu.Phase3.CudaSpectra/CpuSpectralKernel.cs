using Gu.Core;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// CPU reference implementation of ISpectralKernel.
/// Wraps a LinearizedOperatorBundle to provide the same packed-array interface
/// that the GPU kernel uses. Enables CPU/GPU parity testing.
/// </summary>
public sealed class CpuSpectralKernel : ISpectralKernel
{
    private readonly LinearizedOperatorBundle _bundle;

    public CpuSpectralKernel(LinearizedOperatorBundle bundle)
    {
        _bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
    }

    public int StateDimension => _bundle.StateDimension;
    public int ResidualDimension => _bundle.Jacobian.OutputDimension;

    public void ApplySpectral(ReadOnlySpan<double> v, Span<double> result)
    {
        var input = WrapInput(v, StateDimension);
        var output = _bundle.ApplySpectral(input);
        output.Coefficients.AsSpan().CopyTo(result);
    }

    public void ApplyMass(ReadOnlySpan<double> v, Span<double> result)
    {
        var input = WrapInput(v, StateDimension);
        var output = _bundle.ApplyMass(input);
        output.Coefficients.AsSpan().CopyTo(result);
    }

    public void ApplyJacobian(ReadOnlySpan<double> v, Span<double> result)
    {
        var input = WrapInput(v, StateDimension);
        var output = _bundle.Jacobian.Apply(input);
        output.Coefficients.AsSpan().CopyTo(result);
    }

    public void ApplyAdjoint(ReadOnlySpan<double> w, Span<double> result)
    {
        var input = WrapInput(w, ResidualDimension);
        var output = _bundle.Jacobian.ApplyTranspose(input);
        output.Coefficients.AsSpan().CopyTo(result);
    }

    private FieldTensor WrapInput(ReadOnlySpan<double> data, int dim)
    {
        var coeffs = new double[dim];
        data[..dim].CopyTo(coeffs);
        return new FieldTensor
        {
            Label = "kernel-input",
            Signature = _bundle.Jacobian.InputSignature,
            Coefficients = coeffs,
            Shape = new[] { dim },
        };
    }
}
