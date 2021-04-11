module Signer.``Unwrap workflow test``

open FsUnit.Xunit
open Newtonsoft.Json.Linq
open Signer.Ethereum.Multisig
open TzWatch.Domain
open Xunit
open Signer.Unwrap

let fakeSigner =
    { new EthereumSigner with
        member this.PublicAddress() = "PublicAddress" |> AsyncResult.retn

        member this.Sign bytes = "Signature" |> AsyncResult.retn }

let pack: EthPack = fun _ -> [||] |> AsyncResult.retn


let workflow =
    workflow fakeSigner pack "0x0cFa220dDA04DA22754baA1929798ec5E01A3483"

[<Fact>]
let ``Should build erc20 unwrap`` () =
    async {

        let p: Update =
            { UpdateId = (Operation { OpgHash = "hash"; Counter = 10 })
              Value =
                  EntryPointCall
                      { Entrypoint = "unwrap_erc20"
                        Parameters = JToken.Parse(System.IO.File.ReadAllText "./sample/unwrap_erc20_call.json") } }

        let! result = workflow 100I (UnwrapFromTezosUpdate p)

        match result with
        | Ok v ->
            v
            |> should
                equal
                   (Erc20UnwrapSigned
                       { Level = 100I
                         Call =
                             { LockingContract = "0x0cFa220dDA04DA22754baA1929798ec5E01A3483"
                               SignerAddress = "PublicAddress"
                               Signature = "Signature"
                               Parameters =
                                   { Amount = 100000000000000000000I
                                     Owner = "0xecb2d6583858aae994f4248f8948e35516cfc9cf"
                                     ERC20 = "0xc7ad46e0b8a400bb3c915120d284aafba8fc4735"
                                     OperationId = "hash/10" } } })
        | Error e -> failwith e
    }

[<Fact>]
let ``Should build erc721 unwrap`` () =
    async {

        let p: Update =
            { UpdateId = (Operation { OpgHash = "hash"; Counter = 10 })
              Value =
                  EntryPointCall
                      { Entrypoint = "unwrap_erc721"
                        Parameters = JToken.Parse(System.IO.File.ReadAllText "./sample/unwrap_erc721_call.json") } }

        let! result = workflow 100I (UnwrapFromTezosUpdate p)

        match result with
        | Ok v ->
            v
            |> should
                equal
                   (Erc721UnwrapSigned
                       { Level = 100I
                         Call =
                             { LockingContract = "0x0cFa220dDA04DA22754baA1929798ec5E01A3483"
                               SignerAddress = "PublicAddress"
                               Signature = "Signature"
                               Parameters =
                                   { TokenId = 1337I
                                     Owner = "0xecb2d6583858aae994f4248f8948e35516cfc9cf"
                                     ERC721 = "0xc7ad46e0b8a400bb3c915120d284aafba8fc4735"
                                     OperationId = "hash/10" } } })
        | Error e -> failwith e
    }