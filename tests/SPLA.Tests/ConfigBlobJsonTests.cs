using SPLA.Domain.Settings;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// An opaque plugin blob is stored as YAML but handed to the web settings panel — and back to the
/// plugin — as JSON. YAML scalars in an untyped mapping carry no type of their own, so the crossing
/// is where a `true` can quietly turn into a `"true"` that the plugin's bool property refuses.
/// </summary>
public class ConfigBlobJsonTests
{
    [Fact]
    public void Yaml_scalars_keep_their_type_in_the_json_blob()
    {
        var json = ConfigLoader.BlobToJson(ConfigLoader.DeserializeBlob(
            "trusted_connection: true\ndefault_limit: 10\nserver: CISSQL2017\nquoted: \"true\"\n"));

        Assert.Equal(
            "{\"trusted_connection\":true,\"default_limit\":10,\"server\":\"CISSQL2017\",\"quoted\":\"true\"}",
            json);
    }

    [Fact]
    public void Json_blob_round_trips_back_through_yaml()
    {
        var blob = ConfigLoader.BlobFromJson("{\"trusted_connection\":true,\"default_limit\":10}");
        var json = ConfigLoader.BlobToJson(ConfigLoader.DeserializeBlob(ConfigLoader.SerializeBlob(blob)));

        Assert.Equal("{\"trusted_connection\":true,\"default_limit\":10}", json);
    }
}
