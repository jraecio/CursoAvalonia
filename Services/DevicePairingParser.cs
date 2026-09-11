using CursoAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Web;

namespace CursoAvalonia.Services;

public static class DevicePairingParser
{
    public static ConfiguracaoApi Parse(string urlDispositivo)
    {
        if (string.IsNullOrWhiteSpace(urlDispositivo))
        {
            throw new ArgumentException(
                "A URL do dispositivo não foi informada."
            );
        }

        if (!Uri.TryCreate(
            urlDispositivo,
            UriKind.Absolute,
            out Uri? uri))
        {
            throw new ArgumentException(
                "A URL do dispositivo é inválida."
            );
        }

        ConfiguracaoApi config =
            new ConfiguracaoApi();


        // =============================================
        // BASE URL
        // =============================================

        config.BaseUrl =
            $"{uri.Scheme}://{uri.Host}";


        // =============================================
        // PARÂMETROS DA URL
        // =============================================

        var query =
            HttpUtility.ParseQueryString(
                uri.Query
            );


        config.ClientId =
            query["client_id"] ?? string.Empty;

        config.EmpresaNome =
            query["empresa_name"] ?? string.Empty;

        config.EmpresaCnpj =
            query["empresa_cnpj"] ?? string.Empty;

        config.DeviceName =
            query["device_name"] ?? string.Empty;


        // =============================================
        // VALIDAÇÕES
        // =============================================

        if (string.IsNullOrWhiteSpace(config.ClientId))
        {
            throw new Exception(
                "Client ID não encontrado na URL."
            );
        }

        if (string.IsNullOrWhiteSpace(config.EmpresaNome))
        {
            throw new Exception(
                "Nome da empresa não encontrado na URL."
            );
        }

        if (string.IsNullOrWhiteSpace(config.EmpresaCnpj))
        {
            throw new Exception(
                "CNPJ da empresa não encontrado na URL."
            );
        }

        if (string.IsNullOrWhiteSpace(config.DeviceName))
        {
            throw new Exception(
                "Nome do dispositivo não encontrado na URL."
            );
        }


        return config;
    }
}